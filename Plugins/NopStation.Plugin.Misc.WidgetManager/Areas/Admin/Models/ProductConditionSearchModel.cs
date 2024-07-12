using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Misc.WidgetManager.Areas.Admin.Models;

public partial record ProductConditionSearchModel : BaseSearchModel
{
    public int EntityId { get; set; }

    public string EntityName { get; set; }
}
