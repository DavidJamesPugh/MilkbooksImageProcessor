using MilkbooksImageProcessor.Models;
using MilkbooksImageProcessor.Services.Interfaces;
using SkiaSharp;

namespace MilkbooksImageProcessor.Services
{
    public class ResizeService : IResizeService
    {
        private readonly ILogger<ResizeService> _logger;
        private readonly IWebHostEnvironment _env;
        public ResizeService(ILogger<ResizeService> logger, IWebHostEnvironment env)
        {
            _logger = logger;
            _env = env;
        }
        public bool ResizeImage(ImageVariant fullImage, ImageVariantType imageVariantType)
        {
            try
            {
                var inputPath = fullImage.StoragePath;

                if (!File.Exists(inputPath))
                {
                    _logger.LogError("Input file not found: {Path}", fullImage.StoragePath);
                    return false;
                }

                var targetSize = imageVariantType switch
                {
                    ImageVariantType.Thumbnail => ImageSizes.Thumbnail,
                    ImageVariantType.Small => ImageSizes.Small,
                    _ => throw new ArgumentOutOfRangeException()
                }; 
                var directory = Path.Combine(_env.WebRootPath, "images", $"{targetSize}");
                Directory.CreateDirectory(directory);
                var outputPath = Path.Combine(directory, $"{fullImage.Id}_{targetSize}.jpg");

                if (File.Exists(outputPath))
                {
                    return true;
                }

                var ratio = Math.Min(
                    (float)targetSize / fullImage.Width,
                    (float)targetSize / fullImage.Height
                );

                var width = (int)(fullImage.Width * ratio);
                var height = (int)(fullImage.Height * ratio);


                using var input = File.OpenRead(inputPath);
                using var bitmap = SKBitmap.Decode(input);

                if (bitmap == null)
                {
                    _logger.LogError("Failed to decode image. File may be corrupt: {Path}", fullImage.StoragePath);
                    return false;
                }

                using var resized = bitmap.Resize(
                    new SKImageInfo(width, height),
                    SKFilterQuality.High);

                if (resized == null)
                {
                    _logger.LogError("Resize failed for image {Id}. Width={}, Height = {}", fullImage.Id, width, height);
                    return false;
                }

                using var image = SKImage.FromBitmap(resized);
                using var data = image.Encode(SKEncodedImageFormat.Jpeg, 90);

                using var stream = File.Create(outputPath);
                data.SaveTo(stream);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unexpected error resizing image {Id}",
                    fullImage.Id);
                return false;
            }
        }

    
    }
}
