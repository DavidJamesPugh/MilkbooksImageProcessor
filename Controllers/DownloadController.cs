using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MilkbooksImageProcessor.Models;
using MilkbooksImageProcessor.Services.Interfaces;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Channels;

namespace MilkbooksImageProcessor.Controllers
{
    [ApiController]
    [Route("api/images")]
    public class DownloadController : ControllerBase
    {
        private readonly IDownloadService _downloadService;
        private readonly ILogger<DownloadController> _logger;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public DownloadController(IDownloadService downloadService, ILogger<DownloadController> logger)
        {
            _downloadService = downloadService;
            _logger = logger;
        }

        [HttpGet]
        [EnableRateLimiting("images")]
        public async Task GetImages([FromQuery] string query, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                Response.StatusCode = 400;
                await Response.WriteAsJsonAsync(new { error = "Query required" }, cancellationToken);
                return;
            }

            Response.StatusCode = 200;
            Response.Headers.ContentType = "text/event-stream";
            Response.Headers.CacheControl = "no-cache";
            Response.Headers["X-Accel-Buffering"] = "no";

            var channel = Channel.CreateUnbounded<ImageProgress>(
                new UnboundedChannelOptions { SingleReader = true });

            // Service writes progress events and calls TryComplete() before returning
            var downloadTask = _downloadService.DownloadImagesAsync(query, cancellationToken, channel.Writer);

            await foreach (var p in channel.Reader.ReadAllAsync(cancellationToken))
            {
                await WriteDataAsync(new { type = "progress", current = p.Completed, total = p.Total, image = p.Image }, cancellationToken);
            }

            try
            {
                var result = await downloadTask;
                await WriteDataAsync(new
                {
                    type = "complete",
                    successCount = result.SuccessCount,
                    failureCount = result.FailureCount,
                    isPartialResult = result.IsPartialResult
                }, cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Unsplash API request failed");
                await WriteDataAsync(new { type = "error", error = "Failed to reach the Unsplash API. Please try again." }, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error processing image query");
                await WriteDataAsync(new { type = "error", error = "An unexpected error occurred." }, cancellationToken);
            }
        }

        private async Task WriteDataAsync(object data, CancellationToken ct)
        {
            var json = JsonSerializer.Serialize(data, JsonOptions);
            await Response.WriteAsync($"data: {json}\n\n", ct);
            await Response.Body.FlushAsync(ct);
        }
    }
}
