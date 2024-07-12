using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Widgets.AffiliateStation.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.AffiliateStation.Areas.Admin.Models;
using NopStation.Plugin.Widgets.AffiliateStation.Domains;
using NopStation.Plugin.Widgets.AffiliateStation.Extensions;
using NopStation.Plugin.Widgets.AffiliateStation.Services;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Mvc.Filters;

namespace NopStation.Plugin.Widgets.AffiliateStation.Areas.Admin.Controllers
{
    public class CatalogCommissionController : NopStationAdminController
    {
        #region Fileds

        private readonly IPermissionService _permissionService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly ICatalogCommissionModelFactory _catalogCommissionModelFactory;
        private readonly ICatalogCommissionService _catalogCommissionService;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;

        #endregion

        #region Ctor

        public CatalogCommissionController(IPermissionService permissionService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            ICatalogCommissionModelFactory catalogCommissionModelFactory,
            ICatalogCommissionService catalogCommissionService,
            IProductService productService,
            ICategoryService categoryService,
            IManufacturerService manufacturerService)
        {
            _permissionService = permissionService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _catalogCommissionModelFactory = catalogCommissionModelFactory;
            _catalogCommissionService = catalogCommissionService;
            _productService = productService;
            _categoryService = categoryService;
            _manufacturerService = manufacturerService;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Index()
        {
            if (!await _permissionService.AuthorizeAsync(AffiliateStationPermissionProvider.ManageCatalogCommission))
                return AccessDeniedView();

            return RedirectToAction("ProductList");
        }

        public async Task<IActionResult> ProductList()
        {
            if (!await _permissionService.AuthorizeAsync(AffiliateStationPermissionProvider.ManageCatalogCommission))
                return AccessDeniedView();

            var model = await _catalogCommissionModelFactory.PrepareCatalogCommissionSearchModelAsync(SearchType.Product);

            return View(model.ProductSearchModel);
        }

        public async Task<IActionResult> CategoryList()
        {
            if (!await _permissionService.AuthorizeAsync(AffiliateStationPermissionProvider.ManageCatalogCommission))
                return AccessDeniedView();

            var model = await _catalogCommissionModelFactory.PrepareCatalogCommissionSearchModelAsync(SearchType.Category);

            return View(model.CategorySearchModel);
        }

        public async Task<IActionResult> ManufacturerList()
        {
            if (!await _permissionService.AuthorizeAsync(AffiliateStationPermissionProvider.ManageCatalogCommission))
                return AccessDeniedView();

            var model = await _catalogCommissionModelFactory.PrepareCatalogCommissionSearchModelAsync(SearchType.Manufacturer);

            return View(model.ManufacturerSearchModel);
        }

        public async Task<IActionResult> GetProductList(ProductSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(AffiliateStationPermissionProvider.ManageCatalogCommission))
                return AccessDeniedView();

            var commissionSearchModel = new CatalogCommissionSearchModel();
            commissionSearchModel.SearchType = SearchType.Product;
            commissionSearchModel.ProductSearchModel = searchModel;

            var model = await _catalogCommissionModelFactory.PrepareCatalogCommissionListModelAsync(commissionSearchModel);

            return Json(model);
        }

        public async Task<IActionResult> GetCategoryList(CategorySearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(AffiliateStationPermissionProvider.ManageCatalogCommission))
                return AccessDeniedView();

            var commissionSearchModel = new CatalogCommissionSearchModel();
            commissionSearchModel.SearchType = SearchType.Category;
            commissionSearchModel.CategorySearchModel = searchModel;

            var model = await _catalogCommissionModelFactory.PrepareCatalogCommissionListModelAsync(commissionSearchModel);

            return Json(model);
        }

        public async Task<IActionResult> GetManufacturerList(ManufacturerSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(AffiliateStationPermissionProvider.ManageCatalogCommission))
                return AccessDeniedView();

            var commissionSearchModel = new CatalogCommissionSearchModel();
            commissionSearchModel.SearchType = SearchType.Manufacturer;
            commissionSearchModel.ManufacturerSearchModel = searchModel;

            var model = await _catalogCommissionModelFactory.PrepareCatalogCommissionListModelAsync(commissionSearchModel);

            return Json(model);
        }

        public async Task<IActionResult> Edit(string entityName, int entityid)
        {
            if (!await _permissionService.AuthorizeAsync(AffiliateStationPermissionProvider.ManageCatalogCommission))
                return AccessDeniedView();

            var model = new CatalogCommissionModel();
            switch (entityName)
            {
                case "Category":
                    var category = await _categoryService.GetCategoryByIdAsync(entityid);
                    if (category == null || category.Deleted)
                        return RedirectToAction("CategoryList");

                    model = await _catalogCommissionModelFactory.PrepareCatalogCommissionModelAsync(model, category);
                    break;
                case "Product":
                    var product = await _productService.GetProductByIdAsync(entityid);
                    if (product == null || product.Deleted)
                        return RedirectToAction("ProductList");

                    model = await _catalogCommissionModelFactory.PrepareCatalogCommissionModelAsync(model, product);
                    break;
                case "Manufacturer":
                    var manufacturer = await _manufacturerService.GetManufacturerByIdAsync(entityid);
                    if (manufacturer == null || manufacturer.Deleted)
                        return RedirectToAction("ManufacturerList");

                    model = await _catalogCommissionModelFactory.PrepareCatalogCommissionModelAsync(model, manufacturer);
                    break;
                default:
                    return RedirectToAction("ProductList");
            }

            return View(model.ViewPath, model);
        }

        [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> Edit(CatalogCommissionModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(AffiliateStationPermissionProvider.ManageCatalogCommission))
                return AccessDeniedView();

            CatalogCommission catalogCommission = null;
            var reditectTo = "ProductList";

            switch (model.EntityName)
            {
                case "Category":
                    var category = await _categoryService.GetCategoryByIdAsync(model.EntityId);
                    if (category == null || category.Deleted)
                        return RedirectToAction("CategoryList");

                    reditectTo = "CategoryList";
                    catalogCommission = await _catalogCommissionService.GetCatalogCommissionByEntityAsync(category);
                    break;
                case "Product":
                    var product = await _productService.GetProductByIdAsync(model.EntityId);
                    if (product == null || product.Deleted)
                        return RedirectToAction("ProductList");

                    reditectTo = "ProductList";
                    catalogCommission = await _catalogCommissionService.GetCatalogCommissionByEntityAsync(product);
                    break;
                case "Manufacturer":
                    var manufacturer = await _manufacturerService.GetManufacturerByIdAsync(model.EntityId);
                    if (manufacturer == null || manufacturer.Deleted)
                        return RedirectToAction("ManufacturerList");

                    reditectTo = "ManufacturerList";
                    catalogCommission = await _catalogCommissionService.GetCatalogCommissionByEntityAsync(manufacturer);
                    break;
                default:
                    return RedirectToAction("ProductList");
            }

            if (catalogCommission != null)
            {
                catalogCommission = model.ToEntity(catalogCommission);
                await _catalogCommissionService.UpdateCatalogCommissionAsync(catalogCommission);
            }
            else
            {
                catalogCommission = model.ToEntity<CatalogCommission>();
                await _catalogCommissionService.InsertCatalogCommissionAsync(catalogCommission);
            }

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.AffiliateStation.CatalogCommissions.Updated"));

            if (continueEditing)
                return RedirectToAction("Edit", new { entityid = model.EntityId, entityName = model.EntityName });

            return RedirectToAction(reditectTo);
        }

        [HttpPost]
        public virtual async Task<IActionResult> ProductDelete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(AffiliateStationPermissionProvider.ManageCatalogCommission))
                return AccessDeniedView();

            var commission = await _catalogCommissionService.GetCatalogCommissionByIdAsync(id);

            if (commission != null)
                await _catalogCommissionService.DeleteCatalogCommissionAsync(commission);

            return RedirectToAction("ProductList");
        }

        [HttpPost]
        public virtual async Task<IActionResult> CategoryDelete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(AffiliateStationPermissionProvider.ManageCatalogCommission))
                return AccessDeniedView();

            var commission = await _catalogCommissionService.GetCatalogCommissionByIdAsync(id);

            if (commission != null)
                await _catalogCommissionService.DeleteCatalogCommissionAsync(commission);

            return RedirectToAction("CategoryList");
        }

        [HttpPost]
        public virtual async Task<IActionResult> ManufacturerDelete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(AffiliateStationPermissionProvider.ManageCatalogCommission))
                return AccessDeniedView();

            var commission = await _catalogCommissionService.GetCatalogCommissionByIdAsync(id);

            if (commission != null)
                await _catalogCommissionService.DeleteCatalogCommissionAsync(commission);

            return RedirectToAction("ManufacturerList");
        }

        #endregion
    }
}
