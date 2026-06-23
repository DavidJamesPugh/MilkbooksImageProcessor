using System.Text.Json.Serialization;

namespace MilkbooksImageProcessor.Models
{
    public class UnsplashResponse
    {
        [JsonPropertyName("results")]
        public List<UnsplashImage> Results { get; set; } = [];
    }

    public class UnsplashImage
    {
        [JsonPropertyName("urls")]
        public UnsplashUrls Urls { get; set; } = new();
        [JsonPropertyName("likes")]
        public int Likes { get; set; } = 0;
        [JsonPropertyName("alt_description")]
        public string AltDescription { get; set; } = string.Empty;
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
        [JsonPropertyName("user")]
        public UnsplashUser User { get; set; } = new();
        [JsonPropertyName("width")]
        public int Width { get; set; } = 0;
        [JsonPropertyName("height")]
        public int Height { get; set; } = 0;
    }

    public class UnsplashUrls
    {
        [JsonPropertyName("full")]
        public string Full { get; set; } = string.Empty;
    }

    public class UnsplashUser
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }

}
