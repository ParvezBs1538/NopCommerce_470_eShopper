using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.SmartCarousels.Areas.Admin.Models;

public record SmartCarouselCategoryModel : BaseNopEntityModel
{
    public int CarouselId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartCarousels.Carousels.Categories.Fields.Category")]
    public int CategoryId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartCarousels.Carousels.Categories.Fields.Category")]
    public string CategoryName { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartCarousels.Carousels.Categories.Fields.DisplayOrder")]
    public int DisplayOrder { get; set; }
}
