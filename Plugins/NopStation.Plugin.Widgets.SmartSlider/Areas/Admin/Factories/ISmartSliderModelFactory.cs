using System.Threading.Tasks;
using NopStation.Plugin.Widgets.SmartSliders.Areas.Admin.Models;
using NopStation.Plugin.Widgets.SmartSliders.Domains;

namespace NopStation.Plugin.Widgets.SmartSliders.Areas.Admin.Factories;

public interface ISmartSliderModelFactory
{
    Task<ConfigurationModel> PrepareConfigurationModelAsync();

    Task<SmartSliderSearchModel> PrepareSliderSearchModelAsync(SmartSliderSearchModel sliderSearchModel);

    Task<SmartSliderListModel> PrepareSliderListModelAsync(SmartSliderSearchModel searchModel);

    Task<SmartSliderModel> PrepareSliderModelAsync(SmartSliderModel model, SmartSlider slider, bool excludeProperties = false);

    Task<SmartSliderItemListModel> PrepareSliderItemListModelAsync(SmartSliderItemSearchModel searchModel);

    Task<SmartSliderItemModel> PrepareSliderItemModelAsync(SmartSliderItemModel model, SmartSliderItem sliderItem,
        SmartSlider slider, bool excludeProperties = false);
}