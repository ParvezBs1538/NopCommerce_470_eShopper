using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.ProductBadge.Areas.Admin.Models;

public partial record BadgeCategorySearchModel : BaseSearchModel
{
    public int BadgeId { get; set; }
}