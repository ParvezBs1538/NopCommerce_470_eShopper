using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using NopStation.Plugin.Misc.Core.Components;

namespace NopStation.Plugin.Widgets.ProductRequests.Components
{
    public class ProductRequestHeaderMenuViewComponent : NopStationViewComponent
    {
        private readonly IWorkContext _workContext;
        private readonly ProductRequestSettings _productRequestSettings;

        public ProductRequestHeaderMenuViewComponent(IWorkContext workContext,
            ProductRequestSettings productRequestSettings)
        {
            _workContext = workContext;
            _productRequestSettings = productRequestSettings;
        }

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            if (!_productRequestSettings.IncludeInTopMenu)
                return Content("");

            if (!(await _workContext.GetCurrentCustomerAsync()).HasAccessToProductRequest(_productRequestSettings))
                return Content("");

            return View();
        }
    }
}
