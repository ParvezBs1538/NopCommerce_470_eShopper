using System.Threading.Tasks;
using NopStation.Plugin.Widgets.VendorShop.Areas.Admin.Models;
using NopStation.Plugin.Widgets.VendorShop.Areas.Admin.Models.SliderVendorShop;
using NopStation.Plugin.Widgets.VendorShop.Domains.SliderVendorShop;

namespace NopStation.Plugin.Widgets.VendorShop.Areas.Admin.Factories
{
    public interface ISliderModelFactory
    {
        Task<ConfigurationModel> PrepareConfigurationModelAsync();

        Task<SliderSearchModel> PrepareSliderSearchModelAsync(SliderSearchModel sliderSearchModel);

        Task<SliderListModel> PrepareSliderListModelAsync(SliderSearchModel searchModel);

        Task<SliderModel> PrepareSliderModelAsync(SliderModel model, Slider slider, bool excludeProperties = false);

        Task<SliderItemListModel> PrepareSliderItemListModelAsync(SliderItemSearchModel searchModel);

        Task<SliderItemModel> PrepareSliderItemModelAsync(SliderItemModel model, Slider slider,
            SliderItem sliderItem, bool excludeProperties = false);
    }
}