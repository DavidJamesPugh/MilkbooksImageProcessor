using Microsoft.AspNetCore.Mvc;
using MilkbooksImageProcessor.Services.Interfaces;

namespace MilkbooksImageProcessor.Controllers
{
    [ApiController]
    [Route("api/images")]
    public class DownloadController : ControllerBase
    {
        private readonly IDownloadService _downloadService;

        public DownloadController(IDownloadService downloadService)
        {
            _downloadService = downloadService;
        }

        [HttpGet("{query}")]
        public async Task<IActionResult> GetImages(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest("Query required");

            var images = await _downloadService.DownloadImages(query);

            return Ok(new
            {
                images,
                count = images.Count
            });
        }
    }
}
