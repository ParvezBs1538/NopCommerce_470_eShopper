using Nop.Core.Configuration;

namespace NopStation.Plugin.Widget.BlogNews;

public class BlogNewsSettings : ISettings
{
    public string WidgetZone { get; set; }

    public bool ShowBlogsInStore { get; set; }

    public int BlogPostPictureSize { get; set; }

    public int NumberOfBlogPostsToShow { get; set; }

    public bool ShowNewsInStore { get; set; }

    public int NewsItemPictureSize { get; set; }

    public int NumberOfNewsItemsToShow { get; set; }
}
