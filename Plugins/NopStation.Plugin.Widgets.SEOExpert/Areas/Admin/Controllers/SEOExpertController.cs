using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Services;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Mvc;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Widgets.SEOExpert.Areas.Admin.Models;
using NopStation.Plugin.Widgets.SEOExpert.Domains;
using NopStation.Plugin.Widgets.SEOExpert.Extensions;
using NopStation.Plugin.Widgets.SEOExpert.Services;

namespace NopStation.Plugin.Widgets.SEOExpert.Areas.Admin.Controllers
{
    public class SEOExpertController : NopStationAdminController
    {
        #region Fields

        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly ISEOExpertService _seoExpertService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public SEOExpertController(IStoreContext storeContext,
            ISettingService settingService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            IProductService productService,
            ICategoryService categoryService,
            IManufacturerService manufacturerService,
            ISEOExpertService seoExpertService,
            IWorkContext workContext)
        {
            _storeContext = storeContext;
            _settingService = settingService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _productService = productService;
            _categoryService = categoryService;
            _manufacturerService = manufacturerService;
            _seoExpertService = seoExpertService;
            _workContext = workContext;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(SEOExpertPermissionProvider.ManageSEOExpert))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var settings = await _settingService.LoadSettingAsync<SEOExpertSettings>(storeScope);

            var model = new SEOExpertConfigurationModel()
            {
                OpenAIApiKey = settings.OpenAIApiKey,
                Endpoint = settings.Endpoint,
                ModelName = settings.ModelName,
                RequireAdminApproval = settings.RequireAdminApproval,
                AdditionalInfoWithName = settings.AdditionalInfoWithName,
                AdditionalInfoWithShortDescription = settings.AdditionalInfoWithShortDescription,
                AdditionalInfoWithFullDescription = settings.AdditionalInfoWithFullDescription,
                RegenerateConditionIds = settings.GetRegenerateConditionIds(),
                Temperature = settings.Temperature,
                EnableListGeneration = settings.EnableListGeneration
            };

            model.AvailableRegenerateConditions = (await RegenerateCondition.RegenerateIfNotExistMetaTitle.ToSelectListAsync()).Select(x => new SelectListItem()
            {
                Value = x.Value,
                Text = x.Text
            }).ToList();

            return View(model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(SEOExpertConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(SEOExpertPermissionProvider.ManageSEOExpert))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
                var settings = await _settingService.LoadSettingAsync<SEOExpertSettings>(storeScope);

                settings.OpenAIApiKey = model.OpenAIApiKey;
                settings.Endpoint = model.Endpoint;
                settings.RequireAdminApproval = model.RequireAdminApproval;
                settings.AdditionalInfoWithName = model.AdditionalInfoWithName;
                settings.AdditionalInfoWithShortDescription = model.AdditionalInfoWithShortDescription;
                settings.AdditionalInfoWithFullDescription = model.AdditionalInfoWithFullDescription;
                settings.Temperature = model.Temperature;
                settings.EnableListGeneration = model.EnableListGeneration;

                settings.RegenerateConditionIds = string.Join(',', model.RegenerateConditionIds);

                await _settingService.SaveSettingAsync(settings);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

                return RedirectToAction("Configure");
            }

            model.AvailableRegenerateConditions = (await RegenerateCondition.RegenerateIfNotExistMetaTitle.ToSelectListAsync()).Select(x => new SelectListItem()
            {
                Value = x.Value,
                Text = x.Text
            }).ToList();

            return View(model);
        }

        public async Task<IActionResult> GetSEOContentOfProduct(SEOModel model)
        {
            if (!await _permissionService.AuthorizeAsync(SEOExpertPermissionProvider.ManageSEOExpert))
                return AccessDeniedView();

            if (model.EntityTypeId == (int)SEOEntityType.Product)
            {
                var product = await _productService.GetProductByIdAsync(model.EntityId);
                if (product == null || product.Deleted)
                    return new NullJsonResult();

                var seoContent = await _seoExpertService.GenerateSEOAsync(product);

                if (seoContent != null)
                    return Json(new { Result = true, Data = seoContent });
            }
            else if (model.EntityTypeId == (int)SEOEntityType.Category)
            {
                var category = await _categoryService.GetCategoryByIdAsync(model.EntityId);
                if (category == null || category.Deleted)
                    return new NullJsonResult();

                var seoContent = await _seoExpertService.GenerateSEOAsync(category);

                if (seoContent != null)
                    return Json(new { Result = true, Data = seoContent });
            }
            else if (model.EntityTypeId == (int)SEOEntityType.Manufacturer)
            {
                var manufacturer = await _manufacturerService.GetManufacturerByIdAsync(model.EntityId);
                if (manufacturer == null || manufacturer.Deleted)
                    return new NullJsonResult();

                var seoContent = await _seoExpertService.GenerateSEOAsync(manufacturer);

                if (seoContent != null)
                    return Json(new { Result = true, Data = seoContent });
            }
            return Json(new { Result = false, Message = await _localizationService.GetResourceAsync("Admin.Plugins.NopStation.SEOExpert.GenerateFor.Failed") });

        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> ApplySEOContentOnProduct(SEOModel model)
        {
            if (!await _permissionService.AuthorizeAsync(SEOExpertPermissionProvider.ManageSEOExpert))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var product = await _productService.GetProductByIdAsync(model.EntityId);

                if (product == null || product.Deleted)
                {
                    return Json(new { Result = false, Message = await _localizationService.GetResourceAsync("Admin.Plugins.NopStation.SEOExpert.GenerateFor.Failed") });
                }

                product.MetaTitle = model.MetaTitle;
                product.MetaDescription = model.MetaDescription;
                product.MetaKeywords = model.MetaKeywords;

                await _productService.UpdateProductAsync(product);
            }

            return Json(new { Result = true, Message = await _localizationService.GetResourceAsync("Plugins.NopStation.SEOExpert.GenerateForProduct.Success") });
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> SEOGenerate_All_Products(ProductSearchModel model)
        {
            if (!await _permissionService.AuthorizeAsync(SEOExpertPermissionProvider.ManageSEOExpert))
                return AccessDeniedView();

            //a vendor should have access only to his products
            if (await _workContext.GetCurrentVendorAsync() != null)
            {
                model.SearchVendorId = (await _workContext.GetCurrentVendorAsync()).Id;
            }

            var categoryIds = new List<int> { model.SearchCategoryId };
            //include subcategories
            if (model.SearchIncludeSubCategories && model.SearchCategoryId > 0)
                categoryIds.AddRange(await _categoryService.GetChildCategoryIdsAsync(parentCategoryId: model.SearchCategoryId, showHidden: true));

            //0 - all (according to "ShowHidden" parameter)
            //1 - published only
            //2 - unpublished only
            bool? overridePublished = null;
            if (model.SearchPublishedId == 1)
                overridePublished = true;
            else if (model.SearchPublishedId == 2)
                overridePublished = false;

            var products = await _productService.SearchProductsAsync(0,
                categoryIds: categoryIds,
                manufacturerIds: new List<int> { model.SearchManufacturerId },
                storeId: model.SearchStoreId,
                vendorId: model.SearchVendorId,
                warehouseId: model.SearchWarehouseId,
                productType: model.SearchProductTypeId > 0 ? (ProductType?)model.SearchProductTypeId : null,
                keywords: model.SearchProductName,
                showHidden: true,
                overridePublished: overridePublished);

            try
            {
                await _seoExpertService.GenerateAndUpdateSEOAsync(products);

                return Json(new { Result = true, Message = await _localizationService.GetResourceAsync("Plugins.NopStation.SEOExpert.GenerateForAllProducts.Success") });
            }
            catch (Exception ex)
            {
                return Json(new { Result = false, Message = ex.Message });
            }
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> SEOGenerate_All_Categories(CategorySearchModel model)
        {
            if (!await _permissionService.AuthorizeAsync(SEOExpertPermissionProvider.ManageSEOExpert))
                return AccessDeniedView();

            var categories = await _categoryService.GetAllCategoriesAsync(categoryName: model.SearchCategoryName,
                 showHidden: true,
                 storeId: model.SearchStoreId,
                 overridePublished: model.SearchPublishedId == 0 ? null : (bool?)(model.SearchPublishedId == 1));
            try
            {
                await _seoExpertService.GenerateAndUpdateSEOAsync(categories);

                return Json(new { Result = true, Message = await _localizationService.GetResourceAsync("Plugins.NopStation.SEOExpert.GenerateForAllCategories.Success") });
            }
            catch (Exception ex)
            {
                return Json(new { Result = false, Message = ex.Message });
            }
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> SEOGenerate_All_Manufacturers(ManufacturerSearchModel model)
        {
            if (!await _permissionService.AuthorizeAsync(SEOExpertPermissionProvider.ManageSEOExpert))
                return AccessDeniedView();

            var manufacturers = await _manufacturerService.GetAllManufacturersAsync(showHidden: true,
                manufacturerName: model.SearchManufacturerName,
                storeId: model.SearchStoreId,
                overridePublished: model.SearchPublishedId == 0 ? null : (bool?)(model.SearchPublishedId == 1));
            try
            {
                await _seoExpertService.GenerateAndUpdateSEOAsync(manufacturers);

                return Json(new { Result = true, Message = await _localizationService.GetResourceAsync("Plugins.NopStation.SEOExpert.GenerateForAllManufacturers.Success") });
            }
            catch (Exception ex)
            {
                return Json(new { Result = false, Message = ex.Message });
            }
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> SEOGenerate_Selected_Products(string selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(SEOExpertPermissionProvider.ManageSEOExpert))
                return AccessDeniedView();

            var products = new List<Product>();
            if (selectedIds != null)
            {
                var ids = selectedIds
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x))
                    .ToArray();
                products.AddRange(await _productService.GetProductsByIdsAsync(ids));
            }
            //a vendor should have access only to his products
            if (await _workContext.GetCurrentVendorAsync() != null)
            {
                products = await products.WhereAwait(async p => p.VendorId == (await _workContext.GetCurrentVendorAsync()).Id).ToListAsync();
            }

            try
            {
                await _seoExpertService.GenerateAndUpdateSEOAsync(products);

                return Json(new { Result = true, Message = await _localizationService.GetResourceAsync("Plugins.NopStation.SEOExpert.GenerateForAllProducts.Success") });
            }
            catch (Exception ex)
            {
                return Json(new { Result = false, Message = ex.Message });
            }
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> SEOGenerate_Selected_Categories(string selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(SEOExpertPermissionProvider.ManageSEOExpert))
                return AccessDeniedView();

            var categories = new List<Category>();
            if (selectedIds != null)
            {
                var ids = selectedIds
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x))
                    .ToArray();
                categories.AddRange(await _categoryService.GetCategoriesByIdsAsync(ids));
            }

            try
            {
                await _seoExpertService.GenerateAndUpdateSEOAsync(categories);

                return Json(new { Result = true, Message = await _localizationService.GetResourceAsync("Plugins.NopStation.SEOExpert.GenerateForAllCategories.Success") });
            }
            catch (Exception ex)
            {
                return Json(new { Result = false, Message = ex.Message });
            }
        }
        [EditAccess, HttpPost]
        public async Task<IActionResult> SEOGenerate_Selected_Manufacturers(string selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(SEOExpertPermissionProvider.ManageSEOExpert))
                return AccessDeniedView();

            var manufacturers = new List<Manufacturer>();
            if (selectedIds != null)
            {
                var ids = selectedIds
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x))
                    .ToArray();
                manufacturers.AddRange(await _manufacturerService.GetManufacturersByIdsAsync(ids));
            }
            try
            {
                await _seoExpertService.GenerateAndUpdateSEOAsync(manufacturers);

                return Json(new { Result = true, Message = await _localizationService.GetResourceAsync("Plugins.NopStation.SEOExpert.GenerateForAllManufacturers.Success") });
            }
            catch (Exception ex)
            {
                return Json(new { Result = false, Message = ex.Message });
            }
        }

        #endregion
    }
}