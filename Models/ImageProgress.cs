namespace MilkbooksImageProcessor.Models
{
    public record ImageProgress(int Completed, int Total, ImageResponseItem? Image = null);
}
