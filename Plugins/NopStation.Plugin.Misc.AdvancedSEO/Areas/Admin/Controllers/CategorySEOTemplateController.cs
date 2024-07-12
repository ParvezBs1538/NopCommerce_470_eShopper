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
using NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Models.CategoryCategorySEOTemplateMapping;
using NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Models.CategorySEOTemplate;
using NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Models.CategoryToMap;
using NopStation.Plugin.Misc.AdvancedSEO.Domains;
using NopStation.Plugin.Misc.AdvancedSEO.Services;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;

namespace NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Controllers
{
    public class CategorySEOTemplateController : NopStationAdminController
    {
        private readonly ICategoryService _categoryService;
        #region Fields

        private readonly ICategorySEOTemplateModelFactory _categorySEOTemplateModelFactory;
        private readonly ICategoryCategorySEOTemplateMappingModelFactory _categoryCategorySEOTemplateMappingModelFactory;
        private readonly ICategoryCategorySEOTemplateMappingService _categoryCategorySEOTemplateMappingService;
        private readonly ICategorySEOTemplateService _categorySEOTemplateService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IStoreService _storeService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public CategorySEOTemplateController(
            ICategoryService categoryService,
            ICategorySEOTemplateModelFactory categorySEOTemplateModelFactory,
            ICategoryCategorySEOTemplateMappingModelFactory categoryCategorySEOTemplateMappingModelFactory,
            ICategoryCategorySEOTemplateMappingService categoryCategorySEOTemplateMappingService,
            ICategorySEOTemplateService categorySEOTemplateService,
            ILocalizedEntityService localizedEntityService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            IStoreMappingService storeMappingService,
            IStoreService storeService,
            IWorkContext workContext
            )
        {
            _categoryService = categoryService;
            _categorySEOTemplateModelFactory = categorySEOTemplateModelFactory;
            _categoryCategorySEOTemplateMappingModelFactory = categoryCategorySEOTemplateMappingModelFactory;
            _categoryCategorySEOTemplateMappingService = categoryCategorySEOTemplateMappingService;
            _categorySEOTemplateService = categorySEOTemplateService;
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

        protected virtual async Task SaveStoreMappingsAsync(CategorySEOTemplate categorySEOTemplate, CategorySEOTemplateModel model)
        {
            categorySEOTemplate.LimitedToStores = model.SelectedStoreIds.Any();
            await _categorySEOTemplateService.UpdateCategorySEOTemplateAsync(categorySEOTemplate);

            var existingStoreMappings = await _storeMappingService.GetStoreMappingsAsync(categorySEOTemplate);
            var allStores = await _storeService.GetAllStoresAsync();
            foreach (var store in allStores)
            {
                if (model.SelectedStoreIds.Contains(store.Id))
                {
                    //new store
                    if (!existingStoreMappings.Any(sm => sm.StoreId == store.Id))
                        await _storeMappingService.InsertStoreMappingAsync(categorySEOTemplate, store.Id);
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

        protected virtual async Task UpdateLocalesAsync(CategorySEOTemplate categorySEOTemplate, CategorySEOTemplateModel model)
        {

            foreach (var localized in model.Locales)
            {
                await _localizedEntityService.SaveLocalizedValueAsync(categorySEOTemplate,
                    x => x.SEOTitleTemplate,
                    localized.SEOTitleTemplate,
                    localized.LanguageId);

                await _localizedEntityService.SaveLocalizedValueAsync(categorySEOTemplate,
                    x => x.SEODescriptionTemplate,
                    localized.SEODescriptionTemplate,
                    localized.LanguageId);

                await _localizedEntityService.SaveLocalizedValueAsync(categorySEOTemplate,
                    x => x.SEOKeywordsTemplate,
                    localized.SEOKeywordsTemplate,
                    localized.LanguageId);
            }
        }

        #endregion

        #region Methods

        #region Category SEO Template

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(AdvancedSEOPermissionProvider.ManageAdvancedSEOCategoryTemplates))
                return AccessDeniedView();

            //prepare model
            var model = await _categorySEOTemplateModelFactory.PrepareCategorySEOTemplateSearchModelAsync(new CategorySEOTemplateSearchModel());

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(CategorySEOTemplateSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(AdvancedSEOPermissionProvider.ManageAdvancedSEOCategoryTemplates))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await _categorySEOTemplateModelFactory.PrepareCategorySEOTemplateListModelAsync(searchModel);

            return Json(model);
        }

        #region Create / Edit / Delete

        public virtual async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(AdvancedSEOPermissionProvider.ManageAdvancedSEOCategoryTemplates))
                return AccessDeniedView();

            //prepare model
            var model = await _categorySEOTemplateModelFactory.PrepareCategorySEOTemplateModelAsync(new CategorySEOTemplateModel(), null);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(CategorySEOTemplateModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(AdvancedSEOPermissionProvider.ManageAdvancedSEOCategoryTemplates))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var customer = await _workContext.GetCurrentCustomerAsync();
                var categorySEOTemplate = model.ToEntity<CategorySEOTemplate>();
                categorySEOTemplate.CreatedOnUtc = DateTime.UtcNow;
                categorySEOTemplate.UpdatedOnUtc = DateTime.UtcNow;
                categorySEOTemplate.CreatedByCustomerId = customer.Id;
                categorySEOTemplate.LastUpdatedByCustomerId = customer.Id;
                await _categorySEOTemplateService.InsertCategorySEOTemplateAsync(categorySEOTemplate);

                //stores
                await SaveStoreMappingsAsync(categorySEOTemplate, model);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.Added"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = categorySEOTemplate.Id });
            }

            //prepare model
            model = await _categorySEOTemplateModelFactory.PrepareCategorySEOTemplateModelAsync(model, null, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(AdvancedSEOPermissionProvider.ManageAdvancedSEOCategoryTemplates))
                return AccessDeniedView();

            //try to get a categorySEOTemplate with the specified id
            var categorySEOTemplate = await _categorySEOTemplateService.GetCategorySEOTemplateByIdAsync(id);
            if (categorySEOTemplate == null || categorySEOTemplate.Deleted)
                return RedirectToAction("List");

            //prepare model
            var model = await _categorySEOTemplateModelFactory.PrepareCategorySEOTemplateModelAsync(new CategorySEOTemplateModel(), categorySEOTemplate);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(CategorySEOTemplateModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(AdvancedSEOPermissionProvider.ManageAdvancedSEOCategoryTemplates))
                return AccessDeniedView();

            //try to get a categorySEOTemplate with the specified id
            var categorySEOTemplate = await _categorySEOTemplateService.GetCategorySEOTemplateByIdAsync(model.Id);
            if (categorySEOTemplate == null || categorySEOTemplate.Deleted)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                var customer = await _workContext.GetCurrentCustomerAsync();
                categorySEOTemplate = model.ToEntity(categorySEOTemplate);
                categorySEOTemplate.UpdatedOnUtc = DateTime.UtcNow;
                categorySEOTemplate.LastUpdatedByCustomerId = customer.Id;
                await _categorySEOTemplateService.UpdateCategorySEOTemplateAsync(categorySEOTemplate);

                //stores
                await SaveStoreMappingsAsync(categorySEOTemplate, model);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = categorySEOTemplate.Id });
            }

            //prepare model
            model = await _categorySEOTemplateModelFactory.PrepareCategorySEOTemplateModelAsync(model, categorySEOTemplate, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(AdvancedSEOPermissionProvider.ManageAdvancedSEOCategoryTemplates))
                return AccessDeniedView();

            //try to get a categorySEOTemplate with the specified id
            var categorySEOTemplate = await _categorySEOTemplateService.GetCategorySEOTemplateByIdAsync(id);
            if (categorySEOTemplate == null)
                return RedirectToAction("List");

            await _categorySEOTemplateService.DeleteCategorySEOTemplateAsync(categorySEOTemplate);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Plugin.Misc.AdvancedSEO.CategorySEOTemplate.Deleted"));

            return RedirectToAction("List");
        }

        #endregion

        #endregion

        #region Mapping

        [HttpPost]
        public virtual async Task<IActionResult> CategorySEOTemplateMappingList(CategoryCategorySEOTemplateMappingSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(AdvancedSEOPermissionProvider.ManageAdvancedSEOCategoryTemplates))
                return await AccessDeniedDataTablesJson();

            var categorySEOTemplate = await _categorySEOTemplateService.GetCategorySEOTemplateByIdAsync(searchModel.CategorySEOTemplateId);
            if(categorySEOTemplate == null)
                return BadRequest();

            //prepare model
            var model = await _categoryCategorySEOTemplateMappingModelFactory.PrepareCategoryCategorySEOTemplateMappingListModelAsync(searchModel);

            return Json(model);
        }

        [EditAccessAjax, HttpPost]
        public virtual async Task<ActionResult> CategorySEOTemplateMappingDelete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(AdvancedSEOPermissionProvider.ManageAdvancedSEOCategoryTemplates))
                return await AccessDeniedDataTablesJson();

            var categorySEOTemplateMapping = await _categoryCategorySEOTemplateMappingService.GetCategoryCategorySEOTemplateMappingByIdAsync(id);
            
            if(categorySEOTemplateMapping != null)
                await _categoryCategorySEOTemplateMappingService.DeleteCategoryCategorySEOTemplateMappingAsync(categorySEOTemplateMapping);

            return new NullJsonResult();
        }

        #region Add Category To Mapping

        public virtual async Task<IActionResult> CategoryAddPopup(int categorySEOTemplateId)
        {
            if (!await _permissionService.AuthorizeAsync(AdvancedSEOPermissionProvider.ManageAdvancedSEOCategoryTemplates))
                return AccessDeniedView();


            var categorySEOTemplate = await _categorySEOTemplateService.GetCategorySEOTemplateByIdAsync(categorySEOTemplateId);
            if (categorySEOTemplate == null)
                return BadRequest();

            //prepare model
            var model = await _categoryCategorySEOTemplateMappingModelFactory.PrepareCategoryToMapSearchModelAsync(new CategoryToMapSearchModel(), categorySEOTemplate);
            
            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> CategoryAddPopup(CategoryToMapSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(AdvancedSEOPermissionProvider.ManageAdvancedSEOCategoryTemplates))
                return AccessDeniedView();

            var categorySEOTemplate = await _categorySEOTemplateService.GetCategorySEOTemplateByIdAsync(searchModel.CategorySEOTemplateId);
            if (categorySEOTemplate == null)
                return BadRequest();

            if (searchModel.SelectedCategoryIds.Any())
            {
                foreach (var categoryId in searchModel.SelectedCategoryIds)
                {
                    var category = await _categoryService.GetCategoryByIdAsync(categoryId);
                    if(category == null) 
                        continue;

                    var category_CategorySEOTemplate_Map = await _categoryCategorySEOTemplateMappingService.GetCategoryCategorySEOTemplateMappingAsync(categorySEOTemplate.Id, category.Id);

                    if(category_CategorySEOTemplate_Map != null) 
                        continue;

                    category_CategorySEOTemplate_Map = new CategoryCategorySEOTemplateMapping()
                    {
                        CategoryId = category.Id,
                        CategorySEOTemplateId = categorySEOTemplate.Id,
                    };
                    await _categoryCategorySEOTemplateMappingService.InsertCategoryCategorySEOTemplateMappingAsync(category_CategorySEOTemplate_Map);
                }
            }

            ViewBag.RefreshPage = true;

            return View(new CategoryToMapSearchModel());
        }

        [HttpPost]
        public virtual async Task<IActionResult> GetCategoryListToMap(CategoryToMapSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(AdvancedSEOPermissionProvider.ManageAdvancedSEOCategoryTemplates))
                return await AccessDeniedDataTablesJson();

            var categorySEOTemplate = await _categorySEOTemplateService.GetCategorySEOTemplateByIdAsync(searchModel.CategorySEOTemplateId);
            if(categorySEOTemplate == null)
                return BadRequest();

            //prepare model
            var model = await _categoryCategorySEOTemplateMappingModelFactory.PrepareCategoryToMapListModelAsync(searchModel);

            return Json(model);
        }

        #endregion

        #endregion

        #endregion
    }
}
