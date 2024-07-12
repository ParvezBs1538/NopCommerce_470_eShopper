using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Factories;
using NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Models.ProductProductSEOTemplateMapping;
using NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Models.ProductSEOTemplate;
using NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Models.ProductToMap;
using NopStation.Plugin.Misc.AdvancedSEO.Domains;
using NopStation.Plugin.Misc.AdvancedSEO.Services;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;

namespace NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Controllers
{
    public class ProductSEOTemplateController : NopStationAdminController
    {
        private readonly IProductService _productService;
        #region Fields

        private readonly IProductSEOTemplateModelFactory _productSEOTemplateModelFactory;
        private readonly IProductProductSEOTemplateMappingModelFactory _productProductSEOTemplateMappingModelFactory;
        private readonly IProductProductSEOTemplateMappingService _productProductSEOTemplateMappingService;
        private readonly IProductSEOTemplateService _productSEOTemplateService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IStoreService _storeService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public ProductSEOTemplateController(
            IProductService productService,
            IProductSEOTemplateModelFactory productSEOTemplateModelFactory,
            IProductProductSEOTemplateMappingModelFactory productProductSEOTemplateMappingModelFactory,
            IProductProductSEOTemplateMappingService productProductSEOTemplateMappingService,
            IProductSEOTemplateService productSEOTemplateService,
            ILocalizedEntityService localizedEntityService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            IStoreMappingService storeMappingService,
            IStoreService storeService,
            IWorkContext workContext
            )
        {
            _productService = productService;
            _productSEOTemplateModelFactory = productSEOTemplateModelFactory;
            _productProductSEOTemplateMappingModelFactory = productProductSEOTemplateMappingModelFactory;
            _productProductSEOTemplateMappingService = productProductSEOTemplateMappingService;
            _productSEOTemplateService = productSEOTemplateService;
            _localizedEntityService = localizedEntityService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _storeMappingService = storeMappingService;
            _storeService = storeService;
            _workContext = workContext;
        }

        #endregion

        #region Utilites

        protected virtual async Task SaveStoreMappingsAsync(ProductSEOTemplate productSEOTemplate, ProductSEOTemplateModel model)
        {
            productSEOTemplate.LimitedToStores = model.SelectedStoreIds.Any();
            await _productSEOTemplateService.UpdateProductSEOTemplateAsync(productSEOTemplate);

            var existingStoreMappings = await _storeMappingService.GetStoreMappingsAsync(productSEOTemplate);
            var allStores = await _storeService.GetAllStoresAsync();
            foreach (var store in allStores)
            {
                if (model.SelectedStoreIds.Contains(store.Id))
                {
                    //new store
                    if (!existingStoreMappings.Any(sm => sm.StoreId == store.Id))
                        await _storeMappingService.InsertStoreMappingAsync(productSEOTemplate, store.Id);
                }
                else
                {
                    //remove store
                    var storeMappingToDelete = existingStoreMappings.FirstOrDefault(sm => sm.StoreId == store.Id);
                    if (storeMappingToDelete != null)
                        await _storeMappingService.DeleteStoreMappingAsync(storeMappingToDelete);
                }
            }
        }

        protected virtual async Task UpdateLocalesAsync(ProductSEOTemplate productSEOTemplate, ProductSEOTemplateModel model)
        {

            foreach (var localized in model.Locales)
            {
                await _localizedEntityService.SaveLocalizedValueAsync(productSEOTemplate,
                    x => x.SEOTitleTemplate,
                    localized.SEOTitleTemplate,
                    localized.LanguageId);

                await _localizedEntityService.SaveLocalizedValueAsync(productSEOTemplate,
                    x => x.SEODescriptionTemplate,
                    localized.SEODescriptionTemplate,
                    localized.LanguageId);

                await _localizedEntityService.SaveLocalizedValueAsync(productSEOTemplate,
                    x => x.SEOKeywordsTemplate,
                    localized.SEOKeywordsTemplate,
                    localized.LanguageId);
            }
        }

        #endregion

        #region Methods

        #region Product SEO Template

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(AdvancedSEOPermissionProvider.ManageAdvancedSEOProductTemplates))
                return AccessDeniedView();

            //prepare model
            var model = await _productSEOTemplateModelFactory.PrepareProductSEOTemplateSearchModelAsync(new ProductSEOTemplateSearchModel());

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(ProductSEOTemplateSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(AdvancedSEOPermissionProvider.ManageAdvancedSEOProductTemplates))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await _productSEOTemplateModelFactory.PrepareProductSEOTemplateListModelAsync(searchModel);

            return Json(model);
        }

        #region Create / Edit / Delete

        public virtual async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(AdvancedSEOPermissionProvider.ManageAdvancedSEOProductTemplates))
                return AccessDeniedView();

            //prepare model
            var model = await _productSEOTemplateModelFactory.PrepareProductSEOTemplateModelAsync(new ProductSEOTemplateModel(), null);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(ProductSEOTemplateModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(AdvancedSEOPermissionProvider.ManageAdvancedSEOProductTemplates))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var customer = await _workContext.GetCurrentCustomerAsync();
                var productSEOTemplate = model.ToEntity<ProductSEOTemplate>();
                productSEOTemplate.CreatedOnUtc = DateTime.UtcNow;
                productSEOTemplate.UpdatedOnUtc = DateTime.UtcNow;
                productSEOTemplate.CreatedByCustomerId = customer.Id;
                productSEOTemplate.LastUpdatedByCustomerId = customer.Id;
                await _productSEOTemplateService.InsertProductSEOTemplateAsync(productSEOTemplate);

                //stores
                await SaveStoreMappingsAsync(productSEOTemplate, model);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.Added"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = productSEOTemplate.Id });
            }

            //prepare model
            model = await _productSEOTemplateModelFactory.PrepareProductSEOTemplateModelAsync(model, null, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(AdvancedSEOPermissionProvider.ManageAdvancedSEOProductTemplates))
                return AccessDeniedView();

            //try to get a productSEOTemplate with the specified id
            var productSEOTemplate = await _productSEOTemplateService.GetProductSEOTemplateByIdAsync(id);
            if (productSEOTemplate == null || productSEOTemplate.Deleted)
                return RedirectToAction("List");

            //prepare model
            var model = await _productSEOTemplateModelFactory.PrepareProductSEOTemplateModelAsync(new ProductSEOTemplateModel(), productSEOTemplate);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(ProductSEOTemplateModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(AdvancedSEOPermissionProvider.ManageAdvancedSEOProductTemplates))
                return AccessDeniedView();

            //try to get a productSEOTemplate with the specified id
            var productSEOTemplate = await _productSEOTemplateService.GetProductSEOTemplateByIdAsync(model.Id);
            if (productSEOTemplate == null || productSEOTemplate.Deleted)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                var customer = await _workContext.GetCurrentCustomerAsync();
                productSEOTemplate = model.ToEntity(productSEOTemplate);
                productSEOTemplate.UpdatedOnUtc = DateTime.UtcNow;
                productSEOTemplate.LastUpdatedByCustomerId = customer.Id;
                await _productSEOTemplateService.UpdateProductSEOTemplateAsync(productSEOTemplate);

                //stores
                await SaveStoreMappingsAsync(productSEOTemplate, model);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = productSEOTemplate.Id });
            }

            //prepare model
            model = await _productSEOTemplateModelFactory.PrepareProductSEOTemplateModelAsync(model, productSEOTemplate, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(AdvancedSEOPermissionProvider.ManageAdvancedSEOProductTemplates))
                return AccessDeniedView();

            //try to get a productSEOTemplate with the specified id
            var productSEOTemplate = await _productSEOTemplateService.GetProductSEOTemplateByIdAsync(id);
            if (productSEOTemplate == null)
                return RedirectToAction("List");

            await _productSEOTemplateService.DeleteProductSEOTemplateAsync(productSEOTemplate);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductSEOTemplate.Deleted"));

            return RedirectToAction("List");
        }

        #endregion

        #endregion

        #region Mapping

        [HttpPost]
        public virtual async Task<IActionResult> ProductSEOTemplateMappingList(ProductProductSEOTemplateMappingSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(AdvancedSEOPermissionProvider.ManageAdvancedSEOProductTemplates))
                return await AccessDeniedDataTablesJson();

            var productSEOTemplate = await _productSEOTemplateService.GetProductSEOTemplateByIdAsync(searchModel.ProductSEOTemplateId);
            if (productSEOTemplate == null)
                return BadRequest();

            //prepare model
            var model = await _productProductSEOTemplateMappingModelFactory.PrepareProductProductSEOTemplateMappingListModelAsync(searchModel);

            return Json(model);
        }

        [EditAccessAjax, HttpPost]
        public virtual async Task<ActionResult> ProductSEOTemplateMappingDelete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(AdvancedSEOPermissionProvider.ManageAdvancedSEOProductTemplates))
                return await AccessDeniedDataTablesJson();

            var productSEOTemplateMapping = await _productProductSEOTemplateMappingService.GetProductProductSEOTemplateMappingByIdAsync(id);

            if (productSEOTemplateMapping != null)
                await _productProductSEOTemplateMappingService.DeleteProductProductSEOTemplateMappingAsync(productSEOTemplateMapping);

            return new NullJsonResult();
        }

        #region Add Product To Mapping

        public virtual async Task<IActionResult> ProductAddPopup(int productSEOTemplateId)
        {
            if (!await _permissionService.AuthorizeAsync(AdvancedSEOPermissionProvider.ManageAdvancedSEOProductTemplates))
                return AccessDeniedView();


            var productSEOTemplate = await _productSEOTemplateService.GetProductSEOTemplateByIdAsync(productSEOTemplateId);
            if (productSEOTemplate == null)
                return BadRequest();

            //prepare model
            var model = await _productProductSEOTemplateMappingModelFactory.PrepareProductToMapSearchModelAsync(new ProductToMapSearchModel(), productSEOTemplate);

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> ProductAddPopup(ProductToMapSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(AdvancedSEOPermissionProvider.ManageAdvancedSEOProductTemplates))
                return AccessDeniedView();

            var productSEOTemplate = await _productSEOTemplateService.GetProductSEOTemplateByIdAsync(searchModel.ProductSEOTemplateId);
            if (productSEOTemplate == null)
                return BadRequest();

            if (searchModel.SelectedProductIds.Any())
            {
                foreach (var productId in searchModel.SelectedProductIds)
                {
                    var product = await _productService.GetProductByIdAsync(productId);
                    if (product == null)
                        continue;

                    var product_ProductSEOTemplate_Map = await _productProductSEOTemplateMappingService.GetProductProductSEOTemplateMappingAsync(productSEOTemplate.Id, product.Id);

                    if (product_ProductSEOTemplate_Map != null)
                        continue;

                    product_ProductSEOTemplate_Map = new ProductProductSEOTemplateMapping()
                    {
                        ProductId = product.Id,
                        ProductSEOTemplateId = productSEOTemplate.Id,
                    };
                    await _productProductSEOTemplateMappingService.InsertProductProductSEOTemplateMappingAsync(product_ProductSEOTemplate_Map);
                }
            }

            ViewBag.RefreshPage = true;

            return View(new ProductToMapSearchModel());
        }

        [HttpPost]
        public virtual async Task<IActionResult> GetProductListToMap(ProductToMapSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(AdvancedSEOPermissionProvider.ManageAdvancedSEOProductTemplates))
                return await AccessDeniedDataTablesJson();

            var productSEOTemplate = await _productSEOTemplateService.GetProductSEOTemplateByIdAsync(searchModel.ProductSEOTemplateId);
            if (productSEOTemplate == null)
                return BadRequest();

            //prepare model
            var model = await _productProductSEOTemplateMappingModelFactory.PrepareProductToMapListModelAsync(searchModel);

            return Json(model);
        }

        #endregion

        #endregion

        #endregion
    }
}
