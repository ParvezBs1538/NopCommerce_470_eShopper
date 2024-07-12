using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Widgets.FAQ.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.FAQ.Areas.Admin.Models;
using NopStation.Plugin.Widgets.FAQ.Domains;
using NopStation.Plugin.Widgets.FAQ.Services;

namespace NopStation.Plugin.Widgets.FAQ.Areas.Admin.Controllers
{
    public class FAQCategoryController : NopStationAdminController
    {
        #region Fields

        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IFAQCategoryModelFactory _categoryModelFactory;
        private readonly IStoreService _storeService;
        private readonly IFAQCategoryService _categoryService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IPermissionService _permissionService;

        #endregion

        #region Ctor

        public FAQCategoryController(ILocalizedEntityService localizedEntityService,
            IFAQCategoryModelFactory categoryModelFactory,
            IStoreService storeService,
            IFAQCategoryService categoryService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IStoreMappingService storeMappingService,
            IPermissionService permissionService)
        {
            _localizedEntityService = localizedEntityService;
            _categoryModelFactory = categoryModelFactory;
            _storeService = storeService;
            _categoryService = categoryService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _storeMappingService = storeMappingService;
            _permissionService = permissionService;
        }

        #endregion

        #region Utilities

        protected virtual async Task SaveStoreMappingsAsync(FAQCategory category, FAQCategoryModel model)
        {
            category.LimitedToStores = model.SelectedStoreIds.Any();

            var existingStoreMappings = await _storeMappingService.GetStoreMappingsAsync(category);
            var allStores = await _storeService.GetAllStoresAsync();
            foreach (var store in allStores)
            {
                if (model.SelectedStoreIds.Contains(store.Id))
                {
                    //new store
                    if (existingStoreMappings.Count(sm => sm.StoreId == store.Id) == 0)
                        await _storeMappingService.InsertStoreMappingAsync(category, store.Id);
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

        protected virtual async Task UpdateLocalesAsync(FAQCategory category, FAQCategoryModel model)
        {
            foreach (var localized in model.Locales)
            {
                await _localizedEntityService.SaveLocalizedValueAsync(category,
                         x => x.Name,
                         localized.Name,
                         localized.LanguageId);
                await _localizedEntityService.SaveLocalizedValueAsync(category,
                         x => x.Description,
                         localized.Description,
                         localized.LanguageId);
            }
        }

        #endregion

        #region Methods

        #region List

        public async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(FAQPermissionProvider.ManageCategories))
                return AccessDeniedView();

            var model = await _categoryModelFactory.PrepareFAQCategorySearchModelAsync(new FAQCategorySearchModel());

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> List(FAQCategorySearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(FAQPermissionProvider.ManageCategories))
                return await AccessDeniedDataTablesJson();

            var model = await _categoryModelFactory.PrepareFAQCategoryListModelAsync(searchModel);
            return Json(model);
        }

        #endregion

        #region Create/update/delete

        public async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(FAQPermissionProvider.ManageCategories))
                return AccessDeniedView();

            var model = await _categoryModelFactory.PrepareFAQCategoryModelAsync(new FAQCategoryModel(), null);
            return View(model);
        }

        [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async virtual Task<IActionResult> Create(FAQCategoryModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(FAQPermissionProvider.ManageCategories))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var category = model.ToEntity<FAQCategory>();
                await _categoryService.InsertFAQCategoryAsync(category);

                await UpdateLocalesAsync(category, model);
                await SaveStoreMappingsAsync(category, model);

                await _categoryService.UpdateFAQCategoryAsync(category);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.FAQ.FAQCategories.Created"));

                return continueEditing ?
                    RedirectToAction("Edit", new { id = category.Id }) :
                    RedirectToAction("List");
            }

            model = await _categoryModelFactory.PrepareFAQCategoryModelAsync(model, null);

            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(FAQPermissionProvider.ManageCategories))
                return AccessDeniedView();

            var category = await _categoryService.GetFAQCategoryByIdAsync(id);
            if (category == null || category.Deleted)
                return RedirectToAction("List");

            var model = await _categoryModelFactory.PrepareFAQCategoryModelAsync(null, category);

            return View(model);
        }

        [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async virtual Task<IActionResult> Edit(FAQCategoryModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(FAQPermissionProvider.ManageCategories))
                return AccessDeniedView();

            var category = await _categoryService.GetFAQCategoryByIdAsync(model.Id);
            if (category == null || category.Deleted)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                category = model.ToEntity(category);
                await _categoryService.UpdateFAQCategoryAsync(category);

                await UpdateLocalesAsync(category, model);
                await SaveStoreMappingsAsync(category, model);

                await _categoryService.UpdateFAQCategoryAsync(category);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.FAQ.FAQCategories.Updated"));

                return continueEditing ?
                    RedirectToAction("Edit", new { id = model.Id }) :
                    RedirectToAction("List");
            }

            model = await _categoryModelFactory.PrepareFAQCategoryModelAsync(model, category);
            return View(model);
        }

        [EditAccess, HttpPost]
        public async virtual Task<IActionResult> Delete(FAQCategoryModel model)
        {
            if (!await _permissionService.AuthorizeAsync(FAQPermissionProvider.ManageCategories))
                return AccessDeniedView();

            var category = await _categoryService.GetFAQCategoryByIdAsync(model.Id);
            if (category == null || category.Deleted)
                return RedirectToAction("List");

            await _categoryService.DeleteFAQCategoryAsync(category);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.FAQ.FAQCategories.Deleted"));

            return RedirectToAction("List");
        }

        #endregion

        #endregion
    }
}
