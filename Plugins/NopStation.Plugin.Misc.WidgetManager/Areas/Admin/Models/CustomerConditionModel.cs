using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.WidgetManager.Areas.Admin.Models;

public partial record CustomerConditionModel : BaseNopEntityModel
{
    public string EntityName { get; set; }
    public int EntityId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.WidgetManager.CustomerConditions.Fields.Customer")]
    public string CustomerEmail { get; set; }

    [NopResourceDisplayName("Admin.NopStation.WidgetManager.CustomerConditions.Fields.Customer")]
    public int CustomerId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.WidgetManager.CustomerConditions.Fields.Active")]
    public bool Active { get; set; }
}
