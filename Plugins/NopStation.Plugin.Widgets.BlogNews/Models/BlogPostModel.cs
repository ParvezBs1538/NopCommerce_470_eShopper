using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widget.BlogNews.Models;

public record BlogPostModel : BaseNopEntityModel
{
    public string BodyOverview { get; set; }
    public string CreatedOnUtcStr { get; set; }
    public int TotalComments { get; set; }
    public string PictureUrl { get; set; }
    public string AltAttribute { get; set; }
    public string TitleAttribute { get; set; }
    public string SeName { get; set; }
    public string Title { get; set; }
    public bool AllowComments { get; set; }
}
