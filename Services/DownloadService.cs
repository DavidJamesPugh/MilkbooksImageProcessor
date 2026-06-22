using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.WebUtilities;
using MilkbooksImageProcessor.Models;
using MilkbooksImageProcessor.Services.Interfaces;
using System.Net.Http.Headers;
using System.Text.Json;

namespace MilkbooksImageProcessor.Services
{
    public class DownloadService : IDownloadService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly string _apiKey = string.Empty;
        public DownloadService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
            _apiKey = _config["UNSPLASH_API_KEY"] ?? throw new InvalidOperationException("Missing UNSPLASH_API_KEY"); ;
        }

        public async Task<List<ImageProcessingResult>> DownloadImages(string query)
        {

            List<ImageProcessingResult> images = new List<ImageProcessingResult>();
            //Private as there is no need to expose this method outside the class.


            return images;

        }

        public async Task<List<UnsplashImage>> GETUnsplashImages(string query)
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

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<UnsplashResponse>(json);

            return result?.results?.OrderByDescending(x => x.likes).Take(10).ToList() ?? new List<UnsplashImage>();
        }
    }
}
