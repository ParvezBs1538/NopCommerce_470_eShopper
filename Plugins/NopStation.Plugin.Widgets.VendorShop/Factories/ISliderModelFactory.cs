using System.Threading.Tasks;
using NopStation.Plugin.Widgets.VendorShop.Domains.SliderVendorShop;
using NopStation.Plugin.Widgets.VendorShop.Models.SliderVendorShop;

namespace NopStation.Plugin.Widgets.VendorShop.Factories
{
    public interface ISliderModelFactory
    {
        Task<SliderListModel> PrepareSliderListModelAsync(int widgetZoneId, int vendorId);

        Task<SliderModel> PrepareSliderModelAsync(Slider slider);
    }
}