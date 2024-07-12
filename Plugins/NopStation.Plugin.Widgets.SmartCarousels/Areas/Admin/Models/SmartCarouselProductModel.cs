using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.SmartCarousels.Areas.Admin.Models;

public record SmartCarouselProductModel : BaseNopEntityModel
{
    public int CarouselId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartCarousels.Carousels.Products.Fields.Product")]
    public int ProductId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartCarousels.Carousels.Products.Fields.Product")]
    public string ProductName { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartCarousels.Carousels.Products.Fields.DisplayOrder")]
    public int DisplayOrder { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartCarousels.Carousels.Products.Fields.Picture")]
    public string PictureUrl { get; set; }
}
