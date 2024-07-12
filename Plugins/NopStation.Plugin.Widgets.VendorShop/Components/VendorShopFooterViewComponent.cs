using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.VendorShop.Models;

namespace NopStation.Plugin.Widgets.VendorShop.Components
{
    public class VendorShopFooterViewComponent : NopStationViewComponent
    {
        private readonly VendorShopSettings _vendorShopSettings;

        public VendorShopFooterViewComponent(VendorShopSettings vendorShopSettings)
        {
            _vendorShopSettings = vendorShopSettings;
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            var model = new VendorShopFooterModel
            {
                EnableCarousel = _vendorShopSettings.EnableOCarousel,
                EnableSlider = _vendorShopSettings.EnableSlider,
                EnableProductTabs = _vendorShopSettings.EnableProductTabs
            };

            return View(model);
        }
    }
}
