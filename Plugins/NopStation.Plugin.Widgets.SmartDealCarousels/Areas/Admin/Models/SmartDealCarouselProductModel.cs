using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.SmartDealCarousels.Areas.Admin.Models;

public record SmartDealCarouselProductModel : BaseNopEntityModel
{
    public int CarouselId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartDealCarousels.Carousels.Products.Fields.Product")]
    public int ProductId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartDealCarousels.Carousels.Products.Fields.Product")]
    public string ProductName { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartDealCarousels.Carousels.Products.Fields.DisplayOrder")]
    public int DisplayOrder { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartDealCarousels.Carousels.Products.Fields.Picture")]
    public string PictureUrl { get; set; }
}
