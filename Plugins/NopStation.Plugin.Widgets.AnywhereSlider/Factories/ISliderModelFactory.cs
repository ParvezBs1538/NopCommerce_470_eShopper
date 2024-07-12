using System.Threading.Tasks;
using NopStation.Plugin.Widgets.AnywhereSlider.Domains;
using NopStation.Plugin.Widgets.AnywhereSlider.Models;

namespace NopStation.Plugin.Widgets.AnywhereSlider.Factories
{
    public interface ISliderModelFactory
    {
        Task<SliderListModel> PrepareSliderListModelAsync(int widgetZoneId);

        Task<SliderModel> PrepareSliderModelAsync(Slider slider);
    }
}