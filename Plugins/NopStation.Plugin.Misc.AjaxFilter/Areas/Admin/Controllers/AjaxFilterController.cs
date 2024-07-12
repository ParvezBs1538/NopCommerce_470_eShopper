using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Controllers;
using NopStation.Plugin.Misc.AjaxFilter.Areas.Admin.Factories;
using NopStation.Plugin.Misc.AjaxFilter.Areas.Admin.Models;
using NopStation.Plugin.Misc.AjaxFilter.Areas.Admin.Services;
using NopStation.Plugin.Misc.AjaxFilter.Domains;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;

namespace NopStation.Plugin.Misc.AjaxFilter.Areas.Admin.Controllers
{
    public class AjaxFilterController : NopStationAdminController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly ILanguageService _languageService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly IAjaxFilterModelFactory _ajaxFilterModelFactory;
        private readonly IPermissionService _permissionService;
        private readonly IStaticCacheManager _cacheManager;
        private readonly ISpecificationAttributeModelFactory _specificationAttributeModelFactory;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IAjaxFilterSpecificationAttributeModelFactory _ajaxFilterSpecificationAttributeModelFactory;
        private readonly IAjaxFilterSpecificationAttributeService _ajaxFilterSpecificationAttributeService;
        private readonly ICategoryService _categoryService;
        private readonly IAjaxFilterParentCategoryService _ajaxFilterParentCategoryService;

        #endregion

        #region Ctor

        public AjaxFilterController(ILocalizationService localizationService,
            INotificationService notificationService,
            ILanguageService languageService,
            ISettingService settingService,
            IStoreContext storeContext,
            IWorkContext workContext,
            IPermissionService permissionService,
            IAjaxFilterModelFactory ajaxFilterModelFactory,
            ISpecificationAttributeService specificationAttributeService,
            ISpecificationAttributeModelFactory specificationAttributeModelFactory,
            IAjaxFilterSpecificationAttributeModelFactory ajaxFilterSpecificationAttributeModelFactory,
            IAjaxFilterParentCategoryService ajaxFilterParentCategoryService,
            IAjaxFilterSpecificationAttributeService ajaxFilterSpecificationAttributeService,

            ICategoryService categoryService,
            IStaticCacheManager cacheManager)
        {
            _localizationService = localizationService;
            _notificationService = notificationService;
            _languageService = languageService;
            _settingService = settingService;
            _storeContext = storeContext;
            _workContext = workContext;
            _ajaxFilterModelFactory = ajaxFilterModelFactory;
            _permissionService = permissionService;
            _specificationAttributeService = specificationAttributeService;
            _specificationAttributeModelFactory = specificationAttributeModelFactory;
            _ajaxFilterSpecificationAttributeModelFactory = ajaxFilterSpecificationAttributeModelFactory;
            _ajaxFilterSpecificationAttributeService = ajaxFilterSpecificationAttributeService;
            _ajaxFilterParentCategoryService = ajaxFilterParentCategoryService;
            _categoryService = categoryService;
            _cacheManager = cacheManager;
        }

        #endregion

        #region Methods

        #region Configuration

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWidgets))
                return AccessDeniedView();

            var model = await _ajaxFilterModelFactory.PrepareConfigurationModelAsync();

            return View(model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWidgets))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var ajaxFilterSettings = await _settingService.LoadSettingAsync<AjaxFilterSettings>(storeScope);
            ajaxFilterSettings = model.ToSettings(ajaxFilterSettings);

            if (ModelState.IsValid)
            {
                await _settingService.SaveSettingOverridablePerStoreAsync(ajaxFilterSettings, x => x.EnableFilter, model.EnableFilter_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(ajaxFilterSettings, x => x.EnableProductCount, model.EnableProductCount_OverrideForStore, storeScope, false);

                await _settingService.SaveSettingOverridablePerStoreAsync(ajaxFilterSettings, x => x.EnableSpecificationAttributeFilter, model.EnableSpecificationAttributeFilter_OverrideForStore, storeScope, false);

                await _settingService.SaveSettingOverridablePerStoreAsync(ajaxFilterSettings, x => x.CloseCategoryFilterByDefualt, model.CloseCategoryFilterByDefualt_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(ajaxFilterSettings, x => x.EnableCategoryFilter, model.EnableCategoryFilter_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(ajaxFilterSettings, x => x.MaxDisplayForCategories, model.MaxDisplayForCategories_OverrideForStore, storeScope, false);

                await _settingService.SaveSettingOverridablePerStoreAsync(ajaxFilterSettings, x => x.EnableManufacturerFilter, model.EnableManufacturerFilter_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(ajaxFilterSettings, x => x.CloseManufactureFilterByDefualt, model.CloseManufactureFilterByDefualt_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(ajaxFilterSettings, x => x.MaxDisplayForManufacturers, model.MaxDisplayForManufacturers_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(ajaxFilterSettings, x => x.HideManufacturerProductCount, model.HideManufacturerProductCount_OverrideForStore, storeScope, false);

                await _settingService.SaveSettingOverridablePerStoreAsync(ajaxFilterSettings, x => x.EnablePriceRangeFilter, model.EnablePriceRangeFilter_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(ajaxFilterSettings, x => x.ClosePriceRangeFilterByDefualt, model.ClosePriceRangeFilterByDefualt_OverrideForStore, storeScope, false);

                await _settingService.SaveSettingOverridablePerStoreAsync(ajaxFilterSettings, x => x.EnableProductTagsFilter, model.EnableProductTagsFilter_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(ajaxFilterSettings, x => x.CloseProductTagsFilterByDefualt, model.CloseProductTagsFilterByDefualt_OverrideForStore, storeScope, false);

                await _settingService.SaveSettingOverridablePerStoreAsync(ajaxFilterSettings, x => x.EnableMiscFilter, model.EnableMiscFilter_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(ajaxFilterSettings, x => x.CloseMiscFilterByDefualt, model.CloseMiscFilterByDefualt_OverrideForStore, storeScope, false);

                await _settingService.SaveSettingOverridablePerStoreAsync(ajaxFilterSettings, x => x.EnableProductAttributeFilter, model.EnableProductAttributeFilter_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(ajaxFilterSettings, x => x.CloseProductAttributeFilterByDefualt, model.CloseProductAttributeFilterByDefualt_OverrideForStore, storeScope, false);

                await _settingService.SaveSettingOverridablePerStoreAsync(ajaxFilterSettings, x => x.EnableProductRatingsFilter, model.EnableProductRatingsFilter_OverrideForStore, storeScope, false);
                await _settingService.SaveSettingOverridablePerStoreAsync(ajaxFilterSettings, x => x.CloseProductRatingsFilterByDefualt, model.CloseProductRatingsFilterByDefualt_OverrideForStore, storeScope, false);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));
            }

            await _settingService.ClearCacheAsync();

            var configurationModel = await _ajaxFilterModelFactory.PrepareConfigurationModelAsync();

            return View(configurationModel);
        }

        #endregion

        #region SpecificationAttributes

        public virtual async Task<IActionResult> SpecificationAttributePopup()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            var model = new ConfigurationModel();
            return View(model);
        }

        [EditAccess, HttpPost]
        [FormValueRequired("save")]
        public async Task<IActionResult> SpecificationAttributePopup(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

            if (model.SelectedSpecificationAttributeIds.Any())
            {
                foreach (var item in model.SelectedSpecificationAttributeIds)
                {
                    var specificationAttribute = await _specificationAttributeService.GetSpecificationAttributeByIdAsync(item);
                    if (specificationAttribute == null)
                        continue;

                    var sa = await _ajaxFilterSpecificationAttributeService.GetSpecificationAttributeByNameAsync(specificationAttribute.Name);
                    if (sa == null)
                    {
                        var ajaxFilterSpecificationAttribute = new AjaxFilterSpecificationAttribute()
                        {
                            SpecificationId = specificationAttribute.Id,
                            MaxSpecificationAttributesToDisplay = 100,
                            AlternateName = "",
                            DisplayOrder = 0
                        };
                        await _ajaxFilterSpecificationAttributeService.InsertAjaxFilterSpecificationAttributeAsync(ajaxFilterSpecificationAttribute);
                        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));
                    }
                }
            }

            ViewBag.RefreshPage = true;
            return View(new ConfigurationModel());
        }

        public virtual async Task<IActionResult> EditAjaxFilterSpecificationAttribute(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

            var specificationAttribute = await _ajaxFilterSpecificationAttributeService.GetSpecificationAttributeByIdAsync(id);

            if (specificationAttribute == null)
            {
                return RedirectToAction("Configure");
            }

            var model = await _ajaxFilterSpecificationAttributeModelFactory.PrepareAjaxFilterSpecificationAttributeAsync(specificationAttribute);

            return View(model);
        }

        [EditAccess, HttpPost]
        public virtual async Task<IActionResult> EditAjaxFilterSpecificationAttribute(AjaxFilterSpecificationAttributeModel ajaxFilterSpecificationAttributeModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

            var specificationAttribute = await _ajaxFilterSpecificationAttributeService.GetSpecificationAttributeByIdAsync(ajaxFilterSpecificationAttributeModel.Id);

            if (specificationAttribute == null)
            {
                ModelState.AddModelError("Invalid.SpecificationAttribute.Error", "Invalid Specification Attribute Selected");

            }

            if (!ModelState.IsValid)
            {
                return View(ajaxFilterSpecificationAttributeModel);
            }

            specificationAttribute = await _ajaxFilterSpecificationAttributeModelFactory.PrepareSpecificationAttributeAsync(ajaxFilterSpecificationAttributeModel);

            await _ajaxFilterSpecificationAttributeService.UpdateAjaxFilterSpecificationAttribute(specificationAttribute);
            await _cacheManager.RemoveByPrefixAsync(AjaxFilterDefaults.AjaxFilterAvailableSpecificationAttributesPrefix);
            await _cacheManager.RemoveByPrefixAsync(AjaxFilterDefaults.AllAjaxFilterSpecificationAttributeIdsFromCategoryIdPrefix);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }

        [EditAccessAjax, HttpPost]
        public virtual async Task<IActionResult> GetAjaxFilterSpecificationAttributesAsync(AjaxFilterSpecificationAttributeSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
                return await AccessDeniedDataTablesJson();

            if (searchModel.SearchSpecificationAttributeName == null)
            {
                searchModel.SearchSpecificationAttributeName = "";
            }

            var model = await _ajaxFilterSpecificationAttributeModelFactory.PrepareAjaxFilterSpecificationAttributeListModelAsync(searchModel);

            return Json(model);
        }

        [EditAccessAjax, HttpPost]
        public virtual async Task<IActionResult> SpecificationAttributeList(AjaxFilterSpecificationAttributeSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

            if (searchModel.SearchSpecificationAttributeName == null)
            {
                searchModel.SearchSpecificationAttributeName = "";
            }

            var model = await _ajaxFilterSpecificationAttributeModelFactory.PrepareSpecificationAttributeListModelAsync(searchModel);

            return Json(model);
        }

        [EditAccessAjax, HttpPost]
        public virtual async Task<IActionResult> DeleteAjaxFilterSpecificationAttributeAsync(AjaxFilterSpecificationAttributeModel ajaxFilterSpecificationAttributeModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageAttributes))
                return AccessDeniedView();

            if (ajaxFilterSpecificationAttributeModel != null)
            {

                var ajaxFilterSpecificationAttribute = await _ajaxFilterSpecificationAttributeService.GetSpecificationAttributeByIdAsync(ajaxFilterSpecificationAttributeModel.Id);
                if (ajaxFilterSpecificationAttribute != null)
                {
                    await _ajaxFilterSpecificationAttributeService.DeleteAjaxFilterSpecificationAttribute(ajaxFilterSpecificationAttribute);
                }
            }

            await _cacheManager.RemoveByPrefixAsync(AjaxFilterDefaults.AjaxFilterAvailableSpecificationAttributesPrefix);

            return Json(Ok());

        }

        #endregion

        #region ParentCategory

        public async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWidgets))
                return AccessDeniedView();

            var searchModel = new AjaxFilterParentCategorySearchModel();

            searchModel.SearchParentCategoryName = "";

            var model = await _ajaxFilterModelFactory.PrepareParentCategorySearchModelAsync(searchModel);

            return View(model);
        }

        [EditAccessAjax, HttpPost]
        public async Task<IActionResult> GetSelectedParentCategories(AjaxFilterParentCategorySearchModel searchModel)
        {

            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWidgets))
                return AccessDeniedView();

            if (searchModel.SearchParentCategoryName == null)
            {
                searchModel.SearchParentCategoryName = "";
            }
            var model = await _ajaxFilterModelFactory.PrepareSelectedParentCategoryListModelAsync(searchModel);

            return Json(model);
        }

        public virtual async Task<IActionResult> ParentCategoryPopup()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCategories))
                return AccessDeniedView();

            var searchModel = new AjaxFilterParentCategorySearchModel();

            searchModel.SearchParentCategoryName = "";

            var model = await _ajaxFilterModelFactory.PrepareParentCategorySearchModelAsync(searchModel);

            return View(model);
        }

        [EditAccess, HttpPost]
        [FormValueRequired("save")]
        public async Task<IActionResult> ParentCategoryPopup(AddParentCategoryToAjaxFilterModel listSearchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWidgets))
                return AccessDeniedView();

            if (listSearchModel.SelectedParentCategoryIds.Any())
            {
                foreach (var categoryId in listSearchModel.SelectedParentCategoryIds)
                {
                    var parentCategory = await _ajaxFilterParentCategoryService.GetParentCategoryByCategoryIdAsync(categoryId);
                    if (parentCategory == null)
                    {
                        var category = await _categoryService.GetCategoryByIdAsync(categoryId);
                        var model = new AjaxFilterParentCategory();
                        model.CategoryId = category.Id;
                        model.EnableManufactureFiltering = false;
                        model.EnablePriceRangeFiltering = true;
                        model.EnableSpecificationAttributeFiltering = true;
                        model.EnableVendorFiltering = true;
                        model.EnableSearchForSpecifications = true;
                        model.EnableSearchForManufacturers = false;
                        await _ajaxFilterParentCategoryService.InsertParentCategoryAsync(model);
                    }
                    else
                    {
                        continue;
                    }
                }
            }

            ViewBag.RefreshPage = true;

            return View(new AjaxFilterParentCategorySearchModel());
        }


        [EditAccess, HttpPost]
        public async Task<IActionResult> GetExistingParentCategoryPopup(AjaxFilterParentCategorySearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWidgets))
                return AccessDeniedView();

            if (searchModel.SearchParentCategoryName == null)
            {
                searchModel.SearchParentCategoryName = "";
            }

            var model = await _ajaxFilterModelFactory.PrepareParentCategoryListModelAsync(searchModel);

            return Json(model);
        }

        #endregion

        #region ParentCategoryConfiguration 

        public async Task<IActionResult> EditParentCategoryAsync(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWidgets))
                return AccessDeniedView();

            var parentCategory = await _ajaxFilterParentCategoryService.GetParentCategoryByIdAsync(id);

            if (parentCategory != null)
            {
                var model = await _ajaxFilterModelFactory.PrepareParentCategoryModelAsync(parentCategory);
                return View(model);
            }
            else
            {
                return RedirectToAction("List");
            }
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> EditParentCategoryAsync(AjaxFilterParentCategoryModel parentCategoryModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWidgets))
                return AccessDeniedView();

            var category = await _ajaxFilterParentCategoryService.GetParentCategoryByIdAsync(parentCategoryModel.Id);

            if (category == null)
            {
                ModelState.AddModelError("Invalid.Category.Passed", "Invalid Parent Category Selected");
                return View(parentCategoryModel);
            }


            if (ModelState.IsValid)
            {
                var parentCategory = parentCategoryModel.ToEntity(category);
                await _ajaxFilterParentCategoryService.UpdateParentCategoryAsync(parentCategory);
            }

            return RedirectToAction("List");
        }

        [EditAccessAjax, HttpPost]
        public async Task<IActionResult> DeleteSelectedParentCategories(AjaxFilterParentCategory parentCategory)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWidgets))
                return AccessDeniedView();

            var ajaxFilterparentCategory = await _ajaxFilterParentCategoryService.GetParentCategoryByIdAsync(parentCategory.Id);

            if (ajaxFilterparentCategory != null)
            {
                await _ajaxFilterParentCategoryService.DeleteParentCategoryAsync(ajaxFilterparentCategory);
            }

            return Json(Ok());
        }

        #endregion

        #endregion
    }
}
