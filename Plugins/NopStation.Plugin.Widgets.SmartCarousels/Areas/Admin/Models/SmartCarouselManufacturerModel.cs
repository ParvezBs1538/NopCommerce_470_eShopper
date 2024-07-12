using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.SmartCarousels.Areas.Admin.Models;

public record SmartCarouselManufacturerModel : BaseNopEntityModel
{
    public int CarouselId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartCarousels.Carousels.Manufacturers.Fields.Manufacturer")]
    public int ManufacturerId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartCarousels.Carousels.Manufacturers.Fields.Manufacturer")]
    public string ManufacturerName { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartCarousels.Carousels.Manufacturers.Fields.DisplayOrder")]
    public int DisplayOrder { get; set; }
}
