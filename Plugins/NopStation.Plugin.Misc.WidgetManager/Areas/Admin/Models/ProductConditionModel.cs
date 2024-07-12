using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.WidgetManager.Areas.Admin.Models;

public partial record ProductConditionModel : BaseNopEntityModel
{
    public string EntityName { get; set; }
    public int EntityId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.WidgetManager.ProductConditions.Fields.Product")]
    public string ProductName { get; set; }

    [NopResourceDisplayName("Admin.NopStation.WidgetManager.ProductConditions.Fields.Product")]
    public int ProductId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.WidgetManager.ProductConditions.Fields.Published")]
    public bool Published { get; set; }
}
