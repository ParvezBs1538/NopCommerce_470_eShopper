using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.ProductBadge.Areas.Admin.Models;

public partial record BadgeVendorSearchModel : BaseSearchModel
{
    public int BadgeId { get; set; }
}