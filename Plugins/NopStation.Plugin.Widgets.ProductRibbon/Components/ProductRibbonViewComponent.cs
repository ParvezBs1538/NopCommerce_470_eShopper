using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Catalog;
using Nop.Web.Models.Catalog;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.ProductRibbon.Factories;

namespace NopStation.Plugin.Widgets.ProductRibbon.Components
{
    public class ProductRibbonViewComponent : NopStationViewComponent
    {
        #region Fields

        private readonly IProductService _productService;
        private readonly IProductRibbonModelFactory _productRibbonModelFactory;

        #endregion

        #region Ctor

        public ProductRibbonViewComponent(IProductService prdouctService,
            IProductRibbonModelFactory productRibbonModelFactory)
        {
            _productService = prdouctService;
            _productRibbonModelFactory = productRibbonModelFactory;
        }

        #endregion

        #region Method

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData = null)
        {
            var productId = 0;
            if (additionalData.GetType() == typeof(ProductDetailsModel))
            {
                var m = additionalData as ProductDetailsModel;
                productId = m.Id;
            }
            else if (additionalData.GetType() == typeof(ProductOverviewModel))
            {
                var m = additionalData as ProductOverviewModel;
                productId = m.Id;
            }
            else if (additionalData.GetType() == typeof(int))
            {
                productId = Convert.ToInt32(additionalData);
            }

            if (productId == 0)
                return Content("");

            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
                return Content("");

            var model = await _productRibbonModelFactory.PrepareProductRibbonModelAsync(product);

            return View(model);
        }

        #endregion
    }
}
