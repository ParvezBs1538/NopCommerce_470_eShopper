using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Catalog;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Models.Catalog;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.SEOExpert.Areas.Admin.Models;
using NopStation.Plugin.Widgets.SEOExpert.Domains;

namespace NopStation.Plugin.Widgets.SEOExpert.Areas.Admin.Components
{
    public class DetailsPageSEOViewComponent : NopStationViewComponent
    {
        #region Fields

        private readonly IProductService _productService;
        private readonly IPermissionService _permissionService;

        #endregion

        #region Ctor

        public DetailsPageSEOViewComponent(IProductService productService,
            IPermissionService permissionService)
        {
            _productService = productService;
            _permissionService = permissionService;
        }

        #endregion

        #region Methods

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            if (!await _permissionService.AuthorizeAsync(SEOExpertPermissionProvider.ManageSEOExpert))
                return Content("");
            var model = new SEOModel();

            if (additionalData.GetType() == typeof(ProductModel))
            {
                var productModel = additionalData as ProductModel;
                if (productModel != null)
                {
                    model.EntityTypeId = (int)SEOEntityType.Product;
                    model.EntityId = productModel.Id;
                }
            }
            else if (additionalData.GetType() == typeof(CategoryModel))
            {
                var categoryModel = additionalData as CategoryModel;
                if (categoryModel != null)
                {
                    model.EntityTypeId = (int)SEOEntityType.Category;
                    model.EntityId = categoryModel.Id;
                }
            }
            else if (additionalData.GetType() == typeof(ManufacturerModel))
            {
                var manufacturerModel = additionalData as ManufacturerModel;
                if (manufacturerModel != null)
                {
                    model.EntityTypeId = (int)SEOEntityType.Manufacturer;
                    model.EntityId = manufacturerModel.Id;
                }
            }
            return View(model);
        }
        #endregion
    }
}