using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.SmartSliders.Areas.Admin.Models;

public record SmartSliderItemSearchModel : BaseSearchModel
{
    public int SliderId { get; set; }
}
