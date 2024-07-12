using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.ProductBadge.Areas.Admin.Models;

public partial record BadgeProductModel : BaseNopEntityModel
{
    public int ProductId { get; set; }

    public string ProductName { get; set; }

    public bool Published { get; set; }
}