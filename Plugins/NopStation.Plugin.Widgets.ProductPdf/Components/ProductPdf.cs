using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.ProductPdf.Models;
using Nop.Web.Models.Catalog;

namespace NopStation.Plugin.Widgets.ProductPdf.Components
{
    public class ProductPdfViewComponent : NopStationViewComponent
    {
        #region Fields

        private readonly ProductPdfSettings _productPdfSettings;

        #endregion

        #region Ctor

        public ProductPdfViewComponent(ProductPdfSettings productPdfSettings)
        {
            _productPdfSettings = productPdfSettings;
        }

        #endregion

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            if (!_productPdfSettings.EnablePlugin)
                return Content("");

            if (additionalData.GetType() != typeof(ProductDetailsModel))
                return Content("");

            var model = new PublicInfoModel()
            {
                ProductId = (additionalData as ProductDetailsModel).Id
            };

            return View(model);
        }
    }
}
