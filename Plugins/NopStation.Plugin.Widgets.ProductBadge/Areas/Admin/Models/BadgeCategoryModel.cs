using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.ProductBadge.Areas.Admin.Models;

public partial record BadgeCategoryModel : BaseNopEntityModel
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; }

    public bool Published { get; set; }
}