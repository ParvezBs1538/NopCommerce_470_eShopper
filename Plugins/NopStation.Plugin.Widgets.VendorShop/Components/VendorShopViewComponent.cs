using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Web.Models.Catalog;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.VendorShop.Domains.OCarouselVendorShop;
using NopStation.Plugin.Widgets.VendorShop.Factories;
using NopStation.Plugin.Widgets.VendorShop.Helpers;
using NopStation.Plugin.Widgets.VendorShop.Models;
using NopStation.Plugin.Widgets.VendorShop.Services;

namespace NopStation.Plugin.Widgets.VendorShop.Components
{
    public class VendorShopViewComponent : NopStationViewComponent
    {
        private readonly VendorShopSettings _vendorShopSettings;
        private readonly ISliderModelFactory _sliderModelFactory;
        private readonly IOCarouselModelFactory _carouselModelFactory;
        private readonly IOCarouselService _carouselService;
        private readonly IStoreContext _storeContext;
        private readonly IProductTabModelFactory _productTabModelFactory;

        public VendorShopViewComponent(VendorShopSettings vendorShopSettings,
            ISliderModelFactory sliderModelFactory,
            IOCarouselModelFactory carouselModelFactory,
            IOCarouselService carouselService,
            IStoreContext storeContext,
            IProductTabModelFactory productTabModelFactory)
        {
            _vendorShopSettings = vendorShopSettings;
            _sliderModelFactory = sliderModelFactory;
            _carouselModelFactory = carouselModelFactory;
            _carouselService = carouselService;
            _storeContext = storeContext;
            _productTabModelFactory = productTabModelFactory;
        }
        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            if (!WidgetZonelHelper.TryGetWidgetZoneId(widgetZone, out var widgetZoneId))
                return Content(string.Empty);
            var vendorModel = (VendorModel)additionalData;
            if (vendorModel == null)
                return Content(string.Empty);
            IList<OCarousel> carousels = (!_vendorShopSettings.EnableOCarousel)
                ? null
                : (await _carouselService.GetAllCarouselsAsync(new List<int> { widgetZoneId },
                storeId: (await _storeContext.GetCurrentStoreAsync()).Id,
                vendorId: vendorModel.Id,
                active: true)).ToList();

            var productTabModels = await _productTabModelFactory.PrepareProductTabListModelAsync(vendorModel.Id, widgetZone);

            var model = new VendorShopComponentModel
            {
                SliderListModel = (!_vendorShopSettings.EnableSlider)
                ? null : await _sliderModelFactory.PrepareSliderListModelAsync(widgetZoneId, vendorModel.Id),
                OCarouselListModel = (carousels != null && carousels.Any()) ? await _carouselModelFactory.PrepareCarouselListModelAsync(carousels) : null,
                ProductTabsModels = productTabModels
            };
            return View(model);
        }
    }
}
