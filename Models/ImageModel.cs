namespace MilkbooksImageProcessor.Models
{
    public class ImageProcessingResult
    {
        public Dictionary<string, ImageVariantModel> Variants { get; set; } = new();
        public bool Success { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }
    public class ImageVariantModel
    {
        public string FilePath { get; set; } = string.Empty;
        public string AltText { get; set; } = string.Empty;
    }
}
