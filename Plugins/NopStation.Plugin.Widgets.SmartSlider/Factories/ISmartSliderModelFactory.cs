using System.Collections.Generic;
using System.Threading.Tasks;
using NopStation.Plugin.Widgets.SmartSliders.Domains;
using NopStation.Plugin.Widgets.SmartSliders.Models;

namespace NopStation.Plugin.Widgets.SmartSliders.Factories;

public interface ISmartSliderModelFactory
{
    Task<IList<SliderOverviewModel>> PrepareSliderAjaxListModelAsync(string widgetZone);

    Task<IList<SliderModel>> PrepareSliderListModelAsync(string widgetZone);

    Task<SliderModel> PrepareSliderModelAsync(SmartSlider slider);
}