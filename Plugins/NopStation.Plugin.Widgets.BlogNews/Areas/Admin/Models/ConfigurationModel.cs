using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widget.BlogNews.Areas.Admin.Models;

public record ConfigurationModel : BaseNopModel, ISettingsModel
{
    [NopResourceDisplayName("NopStation.BlogNews.Configuration.Fields.WidgetZone")]
    public string WidgetZone { get; set; }
    public bool WidgetZone_OverrideForStore { get; set; }

    [NopResourceDisplayName("NopStation.BlogNews.Configuration.Fields.ShowBlogsInStore")]
    public bool ShowBlogsInStore { get; set; }
    public bool ShowBlogsInStore_OverrideForStore { get; set; }

    [NopResourceDisplayName("NopStation.BlogNews.Configuration.Fields.BlogPostPictureSize")]
    public int BlogPostPictureSize { get; set; }
    public bool BlogPostPictureSize_OverrideForStore { get; set; }

    [NopResourceDisplayName("NopStation.BlogNews.Configuration.Fields.NumberOfBlogPostsToShow")]
    public int NumberOfBlogPostsToShow { get; set; }
    public bool NumberOfBlogPostsToShow_OverrideForStore { get; set; }

    [NopResourceDisplayName("NopStation.BlogNews.Configuration.Fields.ShowNewsInStore")]
    public bool ShowNewsInStore { get; set; }
    public bool ShowNewsInStore_OverrideForStore { get; set; }

    [NopResourceDisplayName("NopStation.BlogNews.Configuration.Fields.NewsItemPictureSize")]
    public int NewsItemPictureSize { get; set; }
    public bool NewsItemPictureSize_OverrideForStore { get; set; }

    [NopResourceDisplayName("NopStation.BlogNews.Configuration.Fields.NumberOfNewsItemsToShow")]
    public int NumberOfNewsItemsToShow { get; set; }
    public bool NumberOfNewsItemsToShow_OverrideForStore { get; set; }

    public int ActiveStoreScopeConfiguration { get; set; }
}
