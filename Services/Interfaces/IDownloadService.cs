using MilkbooksImageProcessor.Models;
using System.Threading.Channels;

namespace MilkbooksImageProcessor.Services.Interfaces
{
    public interface IDownloadService
    {
        Task<ImageResponse> DownloadImagesAsync(string query, CancellationToken cancellationToken, ChannelWriter<ImageProgress>? progress = null);
    }
}
