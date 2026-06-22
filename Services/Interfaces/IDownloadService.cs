using MilkbooksImageProcessor.Models;

namespace MilkbooksImageProcessor.Services.Interfaces
{
    public interface IDownloadService
    {
        Task<List<ImageProcessingResult>> DownloadImages(string query);
    }
}
