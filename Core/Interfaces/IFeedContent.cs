namespace Horde.Core.Interfaces
{
    public interface IFeedContent
    {
        string Title { get; set; }
        string Description { get; set; }
        string ImageUrl { get; set; }
        string DeepLink { get; set; }
    }
}
