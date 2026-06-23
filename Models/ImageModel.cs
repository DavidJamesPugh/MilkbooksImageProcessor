namespace MilkbooksImageProcessor.Models
{
    
    public class ImageProcessingResult
    {
        public Dictionary<ImageVariantType, ImageVariant> Variants { get; set; } = new();
        public string Author { get; set; } = string.Empty;
        public int Likes { get; set; }
        public ImageProcessingResult() { }
        public static ImageProcessingResult FromUnsplash(UnsplashImage image)
        {
            return new ImageProcessingResult
            {
                Author = image.User.Name,
                Likes = image.Likes,
                Variants =
                {
                    [ImageVariantType.Full] = new ImageVariant
                    {
                        Id = image.Id,
                        Url = image.Urls.Full,
                        StoragePath = "",
                        AltText = image.AltDescription ?? "",
                        Width = image.Width,
                        Height = image.Height
                    }
                }
            };
        }
    }

    public enum ImageVariantType
    {
        Full,
        Thumbnail,
        Small
    }
    public static class ImageSizes
    {
        public const int Thumbnail = 256;
        public const int Small = 1024;
    }
    public class ImageVariant
    {
        public string Id { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string StoragePath { get; set; } = string.Empty;
        public string AltText { get; set; } = string.Empty;
        public int Width { get; set; } = 0;
        public int Height { get; set; } = 0;

    }
}
