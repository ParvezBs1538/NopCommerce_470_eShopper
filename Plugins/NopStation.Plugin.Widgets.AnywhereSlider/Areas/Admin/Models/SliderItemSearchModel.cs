using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.AnywhereSlider.Areas.Admin.Models
{
    public record SliderItemSearchModel : BaseSearchModel
    {
        public int SliderId { get; set; }
    }
}
