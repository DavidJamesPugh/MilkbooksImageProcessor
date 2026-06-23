namespace MilkbooksImageProcessor.Models
{
    public record ImageResponse(
    List<ImageResponseItem> Images,
    int SuccessCount,
    int FailureCount,
    bool IsPartialResult
);
    public class ImageResponseItem
    {
        public string Id { get; set; } = string.Empty;
        public string ThumbnailUrl { get; set; } = string.Empty;
        public string SmallUrl { get; set; } = string.Empty;
        public string FullUrl { get; set; } = string.Empty;
        public string AltText { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public int Likes { get; set; }
    }
}
