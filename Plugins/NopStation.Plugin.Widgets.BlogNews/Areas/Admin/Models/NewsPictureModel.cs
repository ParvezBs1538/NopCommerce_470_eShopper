using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widget.BlogNews.Areas.Admin.Models;

public record NewsPictureModel : BaseNopEntityModel
{
    [UIHint("Picture")]
    [NopResourceDisplayName("NopStation.BlogNews.News.Fields.Picture")]
    public int PictureId { get; set; }

    [NopResourceDisplayName("NopStation.BlogNews.News.Fields.Picture")]
    public string PictureUrl { get; set; }

    [NopResourceDisplayName("NopStation.BlogNews.News.Fields.OverrideAltAttribute")]
    public string OverrideAltAttribute { get; set; }

    [NopResourceDisplayName("NopStation.BlogNews.News.Fields.OverrideTitleAttribute")]
    public string OverrideTitleAttribute { get; set; }

    [NopResourceDisplayName("NopStation.BlogNews.News.Fields.ShowInStore")]
    public bool ShowInStore { get; set; }
}
