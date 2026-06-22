namespace MilkbooksImageProcessor.Models
{
    public class UnsplashResponse
    {
        public List<UnsplashImage> results { get; set; }
    }

    public class UnsplashImage
    {
        public UnsplashUrls urls { get; set; }
        public int likes { get; set; }
        public string alt_description { get; set; }
        public string id { get; set; }
    }

    public class UnsplashUrls
    {
        public string full { get; set; }
        public string raw { get; set; }
    }
    
}
