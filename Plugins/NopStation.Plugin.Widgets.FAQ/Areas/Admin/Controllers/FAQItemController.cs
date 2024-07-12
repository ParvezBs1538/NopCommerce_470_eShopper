using System;
using System.Collections.Generic;
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
    public class FAQItemController : NopStationAdminController
    {
        #region Fields

        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IFAQItemModelFactory _itemModelFactory;
        private readonly IStoreService _storeService;
        private readonly IFAQItemService _itemService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IPermissionService _permissionService;
        private readonly IFAQCategoryService _categoryService;
        private readonly IFAQTagService _tagService;

        #endregion

        #region Ctor

        public FAQItemController(ILocalizedEntityService localizedEntityService,
            IFAQItemModelFactory itemModelFactory,
            IStoreService storeService,
            IFAQItemService itemService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IStoreMappingService storeMappingService,
            IPermissionService permissionService,
            IFAQCategoryService categoryService,
            IFAQTagService tagService)
        {
            _localizedEntityService = localizedEntityService;
            _itemModelFactory = itemModelFactory;
            _storeService = storeService;
            _itemService = itemService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _storeMappingService = storeMappingService;
            _permissionService = permissionService;
            _categoryService = categoryService;
            _tagService = tagService;
        }

        #endregion

        #region Utilities

        protected virtual async Task SaveCategoryMappingsAsync(FAQItem item, FAQItemModel model)
        {
            var existingFAQItemCategories = await _categoryService.GetFAQItemCategoriesByItemIdAsync(item.Id, true);

            //delete categories
            foreach (var existingFAQItemCategory in existingFAQItemCategories)
                if (!model.SelectedCategoryIds.Contains(existingFAQItemCategory.FAQCategoryId))
                    await _categoryService.DeleteFAQItemCategoryAsync(existingFAQItemCategory);

            //add categories
            foreach (var categoryId in model.SelectedCategoryIds)
            {
                if (_categoryService.FindFAQItemCategory(existingFAQItemCategories, item.Id, categoryId) == null)
                {
                    await _categoryService.InsertFAQItemCategoryAsync(new FAQItemCategory
                    {
                        FAQItemId = item.Id,
                        FAQCategoryId = categoryId
                    });
                }
            }
        }

        protected virtual string[] ParseFAQItemTags(string productTags)
        {
            var result = new List<string>();
            if (string.IsNullOrWhiteSpace(productTags))
                return result.ToArray();

            var values = productTags.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var val in values)
                if (!string.IsNullOrEmpty(val.Trim()))
                    result.Add(val.Trim());

            return result.ToArray();
        }

        protected virtual async Task SaveStoreMappingsAsync(FAQItem item, FAQItemModel model)
        {
            item.LimitedToStores = model.SelectedStoreIds.Any();

            var existingStoreMappings = await _storeMappingService.GetStoreMappingsAsync(item);
            var allStores = await _storeService.GetAllStoresAsync();
            foreach (var store in allStores)
            {
                if (model.SelectedStoreIds.Contains(store.Id))
                {
                    //new store
                    if (existingStoreMappings.Count(sm => sm.StoreId == store.Id) == 0)
                        await _storeMappingService.InsertStoreMappingAsync(item, store.Id);
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

        protected virtual async Task UpdateLocalesAsync(FAQItem item, FAQItemModel model)
        {
            foreach (var localized in model.Locales)
            {
                await _localizedEntityService.SaveLocalizedValueAsync(item,
                         x => x.Answer,
                         localized.Answer,
                         localized.LanguageId);
                await _localizedEntityService.SaveLocalizedValueAsync(item,
                         x => x.Question,
                         localized.Question,
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

            var model = await _itemModelFactory.PrepareFAQItemSearchModelAsync(new FAQItemSearchModel());

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> List(FAQItemSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(FAQPermissionProvider.ManageCategories))
                return await AccessDeniedDataTablesJson();

            var model = await _itemModelFactory.PrepareFAQItemListModelAsync(searchModel);
            return Json(model);
        }

        #endregion

        #region Create/update/delete

        public async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(FAQPermissionProvider.ManageCategories))
                return AccessDeniedView();

            var model = await _itemModelFactory.PrepareFAQItemModelAsync(new FAQItemModel(), null);
            return View(model);
        }

        [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async virtual Task<IActionResult> Create(FAQItemModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(FAQPermissionProvider.ManageCategories))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var item = model.ToEntity<FAQItem>();
                await _itemService.InsertFAQItemAsync(item);

                await UpdateLocalesAsync(item, model);
                await SaveStoreMappingsAsync(item, model);
                await SaveCategoryMappingsAsync(item, model);

                await _itemService.UpdateFAQItemAsync(item);

                //tags
                await _tagService.UpdateFAQItemTagsAsync(item, ParseFAQItemTags(model.FAQTags));

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.FAQ.FAQItems.Created"));

                return continueEditing ?
                    RedirectToAction("Edit", new { id = item.Id }) :
                    RedirectToAction("List");
            }

            model = await _itemModelFactory.PrepareFAQItemModelAsync(model, null);

            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(FAQPermissionProvider.ManageCategories))
                return AccessDeniedView();

            var item = await _itemService.GetFAQItemByIdAsync(id);
            if (item == null || item.Deleted)
                return RedirectToAction("List");

            var model = await _itemModelFactory.PrepareFAQItemModelAsync(null, item);

            return View(model);
        }

        [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async virtual Task<IActionResult> Edit(FAQItemModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(FAQPermissionProvider.ManageCategories))
                return AccessDeniedView();

            var item = await _itemService.GetFAQItemByIdAsync(model.Id);
            if (item == null || item.Deleted)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                item = model.ToEntity(item);
                await _itemService.UpdateFAQItemAsync(item);

                await UpdateLocalesAsync(item, model);
                await SaveStoreMappingsAsync(item, model);
                await SaveCategoryMappingsAsync(item, model);

                await _itemService.UpdateFAQItemAsync(item);

                //tags
                await _tagService.UpdateFAQItemTagsAsync(item, ParseFAQItemTags(model.FAQTags));

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.FAQ.FAQItems.Updated"));

                return continueEditing ?
                    RedirectToAction("Edit", new { id = model.Id }) :
                    RedirectToAction("List");
            }

            model = await _itemModelFactory.PrepareFAQItemModelAsync(model, item);
            return View(model);
        }

        [EditAccess, HttpPost]
        public async virtual Task<IActionResult> Delete(FAQItemModel model)
        {
            if (!await _permissionService.AuthorizeAsync(FAQPermissionProvider.ManageCategories))
                return AccessDeniedView();

            var item = await _itemService.GetFAQItemByIdAsync(model.Id);
            if (item == null || item.Deleted)
                return RedirectToAction("List");

            await _itemService.DeleteFAQItemAsync(item);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.FAQ.FAQItems.Deleted"));

            return RedirectToAction("List");
        }

        #endregion

        #endregion
    }
}
