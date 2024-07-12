using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Misc.AbandonedCarts.Factories;
using Nop.Services.Configuration;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Models.Catalog;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.Product360View.Models;

namespace NopStation.Plugin.Widgets.Product360View.Components
{
    public class Product360ViewComponent : NopStationViewComponent
    {
        #region Fields

        private readonly IProduct360ModelFactory _product360ModelFactory;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctors
        public Product360ViewComponent(IProduct360ModelFactory product360ModelFactory,
            ISettingService settingService,
            IStoreContext storeContext)
        {
            _product360ModelFactory = product360ModelFactory;
            _settingService = settingService;
            _storeContext = storeContext;
        }

        #endregion

        #region Methods

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var product360Settings = await _settingService.LoadSettingAsync<Product360ViewSettings>(storeScope);

            if (product360Settings.IsEnabled != true)
                return Content("");

            if (additionalData is ProductModel model)
            {
                if (model.Id <= 0 || widgetZone != AdminWidgetZones.ProductDetailsBlock)
                    return Content("");
                var imageSetting = await _product360ModelFactory.PrepareImageSetting360ModelAsync(model.Id);

                var product360Model = new Product360Model();
                product360Model.Id = model.Id;
                product360Model.ProductPictureSearchModel.ProductId = model.Id;
                //prepare page parameters
                product360Model.ProductPictureSearchModel.SetGridPageSize();

                // Map Image Setting 
                if (imageSetting != null)
                    product360Model.ImageSetting360Model = imageSetting;

                return View("~/Plugins/NopStation.Plugin.Widgets.Product360View/Views/PictureMapping.cshtml", product360Model);
            }
            else if (additionalData is ProductDetailsModel detailsModel)
            {
                if (detailsModel.Id <= 0 || widgetZone != PublicWidgetZones.ProductDetailsAfterPictures)
                    return Content("");
                var product360Model = await _product360ModelFactory.PrepareImage360DetailsModelAsync(detailsModel.Id);
                if (product360Model.ImageSetting360Model.IsEnabled)
                {
                    product360Model.Id = detailsModel.Id;
                    product360Model.ProductPictureSearchModel.ProductId = detailsModel.Id;
                    return View("~/Plugins/NopStation.Plugin.Widgets.Product360View/Views/Product360View.cshtml", product360Model);
                }
            }
            return Content("");
        }

        #endregion
    }
}
