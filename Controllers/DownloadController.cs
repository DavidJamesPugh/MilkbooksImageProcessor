using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MilkbooksImageProcessor.Models;
using MilkbooksImageProcessor.Services;
using MilkbooksImageProcessor.Services.Interfaces;
using System.IO.Compression;
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
        private readonly RateLimitCounterService _rateLimitCounter;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<DownloadController> _logger;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public DownloadController(
            IDownloadService downloadService,
            RateLimitCounterService rateLimitCounter,
            IWebHostEnvironment env,
            ILogger<DownloadController> logger)
        {
            _downloadService = downloadService;
            _rateLimitCounter = rateLimitCounter;
            _env = env;
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

            _rateLimitCounter.Increment();

            Response.StatusCode = 200;
            Response.Headers.ContentType = "text/event-stream";
            Response.Headers.CacheControl = "no-cache";
            Response.Headers["X-Accel-Buffering"] = "no";

            var channel = Channel.CreateUnbounded<ImageProgress>(
                new UnboundedChannelOptions { SingleReader = true });

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
                    isPartialResult = result.IsPartialResult,
                    requestsRemaining = _rateLimitCounter.Remaining
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

        [HttpGet("zip")]
        public async Task DownloadZip(
            [FromQuery] string ids,
            [FromQuery] string size = "small",
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(ids))
            {
                Response.StatusCode = 400;
                return;
            }

            var imageIds = ids.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var (folder, suffix) = GetFolderAndSuffix(size);

            // Build into MemoryStream first — ZipArchive.Dispose() writes the central
            // directory synchronously, which Kestrel disallows on Response.Body directly.
            using var ms = new MemoryStream();

            using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
            {
                foreach (var id in imageIds)
                {
                    var filePath = Path.Combine(_env.WebRootPath, "images", folder, $"{id}{suffix}");
                    if (!System.IO.File.Exists(filePath)) continue;

                    var entry = zip.CreateEntry($"{id}{suffix}", CompressionLevel.Fastest);
                    await using var entryStream = entry.Open();
                    await using var fileStream = System.IO.File.OpenRead(filePath);
                    await fileStream.CopyToAsync(entryStream, cancellationToken);
                }
            } // Dispose writes central directory synchronously to MemoryStream — allowed

            Response.StatusCode = 200;
            Response.ContentType = "application/zip";
            Response.Headers.ContentDisposition = $"attachment; filename=\"milkbooks_{size}.zip\"";
            Response.ContentLength = ms.Length;

            ms.Position = 0;
            await ms.CopyToAsync(Response.Body, cancellationToken);
        }

        private static (string folder, string suffix) GetFolderAndSuffix(string size) =>
            size.ToLowerInvariant() switch
            {
                "full"  => ("full",  ".jpg"),
                "thumb" => ($"{ImageSizes.Thumbnail}", $"_{ImageSizes.Thumbnail}.jpg"),
                _       => ($"{ImageSizes.Small}",     $"_{ImageSizes.Small}.jpg")
            };

        private async Task WriteDataAsync(object data, CancellationToken ct)
        {
            var json = JsonSerializer.Serialize(data, JsonOptions);
            await Response.WriteAsync($"data: {json}\n\n", ct);
            await Response.Body.FlushAsync(ct);
        }
    }
}
