using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.ProductBadge.Areas.Admin.Models;

public partial record BadgeManufacturerModel : BaseNopEntityModel
{
    public int ManufacturerId { get; set; }

    public string ManufacturerName { get; set; }

    public bool Published { get; set; }
}