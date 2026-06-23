using MilkbooksImageProcessor.Models;

namespace MilkbooksImageProcessor.Services
{
    public class ImageCleanupService : BackgroundService
    {
        private static readonly TimeSpan RunInterval = TimeSpan.FromMinutes(15);
        private static readonly TimeSpan MaxAge = TimeSpan.FromHours(1);

        private readonly IWebHostEnvironment _env;
        private readonly ILogger<ImageCleanupService> _logger;

        public ImageCleanupService(IWebHostEnvironment env, ILogger<ImageCleanupService> logger)
        {
            _env = env;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(RunInterval, stoppingToken);
                DeleteOldFiles();
            }
        }

        private void DeleteOldFiles()
        {
            var subfolders = new[] { "full", $"{ImageSizes.Thumbnail}", $"{ImageSizes.Small}" };
            var cutoff = DateTime.UtcNow - MaxAge;
            var deleted = 0;

            foreach (var folder in subfolders)
            {
                var dir = Path.Combine(_env.WebRootPath, "images", folder);
                if (!Directory.Exists(dir)) continue;

                foreach (var file in Directory.EnumerateFiles(dir, "*.jpg"))
                {
                    try
                    {
                        if (File.GetLastWriteTimeUtc(file) < cutoff)
                        {
                            File.Delete(file);
                            deleted++;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Could not delete image file {File}", file);
                    }
                }
            }

            if (deleted > 0)
                _logger.LogInformation("Image cleanup: deleted {Count} file(s) older than {Age}", deleted, MaxAge);
        }
    }
}
