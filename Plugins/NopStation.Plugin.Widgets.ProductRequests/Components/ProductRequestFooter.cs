using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.ProductRequests.Models;

namespace NopStation.Plugin.Widgets.ProductRequests.Components
{
    public class ProductRequestFooterViewComponent : NopStationViewComponent
    {
        private readonly IWorkContext _workContext;
        private readonly ProductRequestSettings _productRequestSettings;

        public ProductRequestFooterViewComponent(IWorkContext workContext,
            ProductRequestSettings productRequestSettings)
        {
            _workContext = workContext;
            _productRequestSettings = productRequestSettings;
        }

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            if(!_productRequestSettings.IncludeInFooter)
                return Content("");

            if (!(await _workContext.GetCurrentCustomerAsync()).HasAccessToProductRequest(_productRequestSettings))
                return Content("");

            var model = new FooterModel();
            model.FooterElementSelector = _productRequestSettings.FooterElementSelector;
            return View(model);
        }
    }
}
