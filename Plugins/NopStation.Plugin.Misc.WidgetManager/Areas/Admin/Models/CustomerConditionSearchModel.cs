using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Misc.WidgetManager.Areas.Admin.Models;

public partial record CustomerConditionSearchModel : BaseSearchModel
{
    public int EntityId { get; set; }

    public string EntityName { get; set; }
}
