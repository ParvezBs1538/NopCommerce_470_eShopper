using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.ProductTabs.Factories;

namespace NopStation.Plugin.Widgets.ProductTabs.Components
{
    public class ProductTabViewComponent : NopStationViewComponent
    {
        private readonly ProductTabSettings _productTabSettings;
        private readonly IProductTabModelFactory _productTabModelFactory;

        public ProductTabViewComponent(IProductTabModelFactory productTabModelFactory,
            ProductTabSettings productTabSettings)
        {
            _productTabSettings = productTabSettings;
            _productTabModelFactory = productTabModelFactory;
        }

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            if (!_productTabSettings.EnableProductTab)
                return Content("");

            var productTabModels = await _productTabModelFactory.PrepareProductTabListModelAsync(widgetZone);
            return View(productTabModels);
        }
    }
}
