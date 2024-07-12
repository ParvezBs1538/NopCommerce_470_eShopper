using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Models.Catalog;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.ProductQuestionAnswer.Factories;

namespace NopStation.Plugin.Widgets.ProductQuestionAnswer.Components
{
    public class ProductQAViewComponent : NopStationViewComponent
    {
        #region Fields
        private readonly IProductQAModelFactory _productQAModelFactory;
        private readonly ProductQAConfigurationSettings _productQAConfigurationSettings;
        #endregion

        #region Ctor

        public ProductQAViewComponent(IProductQAModelFactory productQAModelFactory,
            ProductQAConfigurationSettings productQAConfigurationSettings)
        {
            _productQAModelFactory = productQAModelFactory;
            _productQAConfigurationSettings = productQAConfigurationSettings;
        }
        #endregion

        #region Methods

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            if (!_productQAConfigurationSettings.IsEnable)
                return Content("");
            ProductDetailsModel productDetailsModel = (ProductDetailsModel)additionalData;
            if (productDetailsModel == null)
                return Content("");

            var model = await _productQAModelFactory.PrepareProductQAPublicInfoModelAsync(productDetailsModel.Id);

            if (model == null)
                return Content("");

            model.ProductQAModel.ProductId = productDetailsModel.Id;
            return View(model);
        }
        #endregion
    }
}
