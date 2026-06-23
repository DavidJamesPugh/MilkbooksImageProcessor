using MilkbooksImageProcessor.Models;

namespace MilkbooksImageProcessor.Services.Interfaces
{
    public interface IResizeService
    {
        bool ResizeImage(ImageVariant fullImage, ImageVariantType imageVariantType);
    }
}
