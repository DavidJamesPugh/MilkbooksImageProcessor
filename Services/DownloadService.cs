using Microsoft.AspNetCore.WebUtilities;
using MilkbooksImageProcessor.Models;
using MilkbooksImageProcessor.Services.Interfaces;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Channels;

namespace MilkbooksImageProcessor.Services
{
    public class DownloadService : IDownloadService
    {
        private readonly HttpClient _httpClient;
        private readonly IResizeService _resizeService;
        private readonly string _apiKey = string.Empty;
        private readonly ILogger<DownloadService> _logger;
        private readonly IWebHostEnvironment _env;
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(3);
        public DownloadService(HttpClient httpClient, 
            IConfiguration config, 
            IResizeService resizeService, 
            ILogger<DownloadService> logger,
            IWebHostEnvironment env)
        {
            _logger = logger;
            _httpClient = httpClient;
            _resizeService = resizeService;
            _apiKey = config["UNSPLASH_API_KEY"] ?? throw new InvalidOperationException("Missing UNSPLASH_API_KEY");
            _env = env;
        }

        public async Task<ImageResponse> DownloadImagesAsync(string query, CancellationToken cancellationToken, ChannelWriter<ImageProgress>? progress = null)
        {
            List<UnsplashImage> unsplashImages = await FetchUnsplashImagesAsync(query, cancellationToken);
            int totalAttempted = unsplashImages.Count;
            int completedCount = 0;

            var imageDownloadTasks = unsplashImages
                .Select(ImageProcessingResult.FromUnsplash)
                .Select(async image =>
                {
                    await _semaphore.WaitAsync();
                    var fullImage = image.Variants[ImageVariantType.Full];
                    ImageResponseItem? responseItem = null;
                    try
                    {
                        var fullPath = await DownloadImageAsync(fullImage);
                        fullImage.StoragePath = fullPath;
                        await Task.Run(() => _resizeService.ResizeImage(fullImage, ImageVariantType.Thumbnail));
                        await Task.Run(() => _resizeService.ResizeImage(fullImage, ImageVariantType.Small));

                        responseItem = new ImageResponseItem
                        {
                            Id = fullImage.Id,
                            ThumbnailUrl = $"/images/256/{fullImage.Id}_256.jpg",
                            SmallUrl = $"/images/1024/{fullImage.Id}_1024.jpg",
                            FullUrl = $"/images/full/{fullImage.Id}.jpg",
                            AltText = fullImage.AltText,
                            Author = image.Author,
                            Likes = image.Likes
                        };
                        return responseItem;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to process image {Id}", fullImage.Id);
                        return null;
                    }
                    finally
                    {
                        _semaphore.Release();
                        var current = Interlocked.Increment(ref completedCount);
                        progress?.TryWrite(new ImageProgress(current, totalAttempted, responseItem));
                    }
                });

            var items = (await Task.WhenAll(imageDownloadTasks))
                .Where(x => x is not null)
                .ToList()!;

            progress?.TryComplete();

            int failureCount = totalAttempted - items.Count;
            return new ImageResponse(items!, items.Count, failureCount, failureCount > 0);
        }

        private async Task<string> DownloadImageAsync(ImageVariant image)
        {
            var directory = Path.Combine(_env.WebRootPath, "images", "full");
            Directory.CreateDirectory(directory);
            var filePath = Path.Combine(directory, $"{image.Id}.jpg");

            if (File.Exists(filePath))
            {
                return filePath;
            }

            using var downloadedImage = await _httpClient.GetAsync(image.Url, HttpCompletionOption.ResponseHeadersRead);
            downloadedImage.EnsureSuccessStatusCode();

            if (!downloadedImage.Content.Headers.ContentType?.MediaType?.StartsWith("image") ?? true)
            {
                _logger.LogError("Non-image response for {Url}", image.Url);
                return string.Empty;
            }

            await using var stream = await downloadedImage.Content.ReadAsStreamAsync();

            await using var fileStream = File.Create(filePath);

            await stream.CopyToAsync(fileStream);

            if (new FileInfo(filePath).Length == 0)
            {
                _logger.LogError("Downloaded file is empty: {Url}", image.Url);
                return string.Empty;
            }

            return filePath;
        }
        private async Task<List<UnsplashImage>> FetchUnsplashImagesAsync(string query, CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Get,
            "https://api.unsplash.com/search/photos");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Client-ID", _apiKey); 

            var queryParams = QueryHelpers.AddQueryString(request.RequestUri!.ToString(),
                new Dictionary<string, string?>
                {
                    ["query"] = query,
                    ["page"] = "1",
                    ["per_page"] = "25"
                });

            request.RequestUri = new Uri(queryParams);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<UnsplashResponse>(json);

            return result?.Results?.OrderByDescending(x => x.Likes).Take(10).ToList() ?? new List<UnsplashImage>();
        }
    }
}
