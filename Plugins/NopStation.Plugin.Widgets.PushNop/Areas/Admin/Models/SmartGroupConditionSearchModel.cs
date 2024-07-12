using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.PushNop.Areas.Admin.Models
{
    public record SmartGroupConditionSearchModel : BaseSearchModel
    {
        public int SmartGroupConditionId { get; set; }
    }
}
