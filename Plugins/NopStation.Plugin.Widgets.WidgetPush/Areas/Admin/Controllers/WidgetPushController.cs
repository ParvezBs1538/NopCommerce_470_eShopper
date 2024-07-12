using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Widgets.WidgetPush.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.WidgetPush.Areas.Admin.Models;
using NopStation.Plugin.Widgets.WidgetPush.Domains;
using NopStation.Plugin.Widgets.WidgetPush.Services;

namespace NopStation.Plugin.Widgets.WidgetPush.Areas.Admin.Controllers
{
    public class WidgetPushController : NopStationAdminController
    {
        #region Firlds

        private readonly IPermissionService _permissionService;
        private readonly IWidgetPushItemService _widgetPushItemService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IWidgetPushItemModelFactory _widgetPushItemModelFactory;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IStoreService _storeService;

        #endregion

        #region Ctor

        public WidgetPushController(IPermissionService permissionService,
            IWidgetPushItemService widgetPushItemService,
            ISettingService settingService,
            IStoreContext storeContext,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IWidgetPushItemModelFactory widgetPushItemModelFactory,
            IDateTimeHelper dateTimeHelper,
            ILocalizedEntityService localizedEntityService,
            IStoreMappingService storeMappingService,
            IStoreService storeService)
        {
            _permissionService = permissionService;
            _widgetPushItemService = widgetPushItemService;
            _settingService = settingService;
            _storeContext = storeContext;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _widgetPushItemModelFactory = widgetPushItemModelFactory;
            _dateTimeHelper = dateTimeHelper;
            _localizedEntityService = localizedEntityService;
            _storeMappingService = storeMappingService;
            _storeService = storeService;
        }

        #endregion

        #region Utilities

        protected virtual async Task SaveStoreMappingsAsync(WidgetPushItem widgetPushItem, WidgetPushItemModel model)
        {
            widgetPushItem.LimitedToStores = model.SelectedStoreIds.Any();

            var existingStoreMappings = await _storeMappingService.GetStoreMappingsAsync(widgetPushItem);
            var allStores = await _storeService.GetAllStoresAsync();
            foreach (var store in allStores)
            {
                if (model.SelectedStoreIds.Contains(store.Id))
                {
                    //new store
                    if (existingStoreMappings.Count(sm => sm.StoreId == store.Id) == 0)
                        await _storeMappingService.InsertStoreMappingAsync(widgetPushItem, store.Id);
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

        protected virtual async Task UpdateLocalesAsync(WidgetPushItem widgetPushItem, WidgetPushItemModel model)
        {
            foreach (var localized in model.Locales)
            {
                await _localizedEntityService.SaveLocalizedValueAsync(widgetPushItem,
                    x => x.Content,
                    localized.Content,
                    localized.LanguageId);
            }
        }

        #endregion

        #region Methods

        #region Configure

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(WidgetPushPermissionProvider.ManageWidgetPush))
                return AccessDeniedView();

            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var widgetPushSettings = await _settingService.LoadSettingAsync<WidgetPushSettings>(storeId);

            var model = new ConfigurationModel()
            {
                HideInPublicStore = widgetPushSettings.HideInPublicStore,
            };

            model.ActiveStoreScopeConfiguration = storeId;

            if (storeId > 0)
            {
                model.HideInPublicStore_OverrideForStore = await _settingService.SettingExistsAsync(widgetPushSettings, x => x.HideInPublicStore, storeId);
            }

            return View(model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(WidgetPushPermissionProvider.ManageWidgetPush))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var widgetPushSettings = await _settingService.LoadSettingAsync<WidgetPushSettings>(storeScope);

            widgetPushSettings.HideInPublicStore = model.HideInPublicStore;

            await _settingService.SaveSettingOverridablePerStoreAsync(widgetPushSettings, x => x.HideInPublicStore, model.HideInPublicStore_OverrideForStore, storeScope, false);

            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }

        #endregion

        #region Widget push item        

        public virtual async Task<IActionResult> Index()
        {
            if (!await _permissionService.AuthorizeAsync(WidgetPushPermissionProvider.ManageWidgetPush))
                return AccessDeniedView();

            return RedirectToAction("List");
        }

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(WidgetPushPermissionProvider.ManageWidgetPush))
                return AccessDeniedView();

            var searchModel = _widgetPushItemModelFactory.PrepareWidgetPushItemSearchModel(new WidgetPushItemSearchModel());
            return View(searchModel);
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(WidgetPushItemSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(WidgetPushPermissionProvider.ManageWidgetPush))
                return await AccessDeniedDataTablesJson();

            var model = await _widgetPushItemModelFactory.PrepareWidgetPushItemListModelAsync(searchModel);
            return Json(model);
        }

        public virtual async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(WidgetPushPermissionProvider.ManageWidgetPush))
                return AccessDeniedView();

            var model = await _widgetPushItemModelFactory.PrepareWidgetPushItemModelAsync(new WidgetPushItemModel(), null);
            return View(model);
        }

        [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Create(WidgetPushItemModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(WidgetPushPermissionProvider.ManageWidgetPush))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var widgetPushItem = new WidgetPushItem()
                {
                    Name = model.Name,
                    Content = model.Content,
                    DisplayOrder = model.DisplayOrder,
                    WidgetZone = model.WidgetZone,
                    Active = model.Active,
                    DisplayEndDateUtc = model.DisplayEndDate.HasValue ?
                        _dateTimeHelper.ConvertToUtcTime(model.DisplayEndDate.Value) : (DateTime?)null,
                    DisplayStartDateUtc = model.DisplayStartDate.HasValue ?
                        _dateTimeHelper.ConvertToUtcTime(model.DisplayStartDate.Value) : (DateTime?)null
                };
                await _widgetPushItemService.InsertWidgetPushItemAsync(widgetPushItem);

                //locales
                await UpdateLocalesAsync(widgetPushItem, model);

                await SaveStoreMappingsAsync(widgetPushItem, model);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.WidgetPush.WidgetPushItems.Created"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = widgetPushItem.Id });
            }

            model = await _widgetPushItemModelFactory.PrepareWidgetPushItemModelAsync(new WidgetPushItemModel(), null);

            return View(model);
        }

        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(WidgetPushPermissionProvider.ManageWidgetPush))
                return AccessDeniedView();

            var widgetPushItem = await _widgetPushItemService.GetWidgetPushItemByIdAsync(id);
            if (widgetPushItem == null)
                return RedirectToAction("List");

            var model = await _widgetPushItemModelFactory.PrepareWidgetPushItemModelAsync(null, widgetPushItem);

            return View(model);
        }

        [EditAccess, HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(WidgetPushItemModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(WidgetPushPermissionProvider.ManageWidgetPush))
                return AccessDeniedView();

            var widgetPushItem = await _widgetPushItemService.GetWidgetPushItemByIdAsync(model.Id);
            if (widgetPushItem == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                widgetPushItem.Name = model.Name;
                widgetPushItem.WidgetZone = model.WidgetZone;
                widgetPushItem.Content = model.Content;
                widgetPushItem.DisplayOrder = model.DisplayOrder;
                widgetPushItem.Active = model.Active;
                widgetPushItem.DisplayEndDateUtc = model.DisplayEndDate.HasValue ?
                    _dateTimeHelper.ConvertToUtcTime(model.DisplayEndDate.Value) : (DateTime?)null;
                widgetPushItem.DisplayStartDateUtc = model.DisplayStartDate.HasValue ?
                    _dateTimeHelper.ConvertToUtcTime(model.DisplayStartDate.Value) : (DateTime?)null;

                await _widgetPushItemService.UpdateWidgetPushItemAsync(widgetPushItem);

                //locales
                await UpdateLocalesAsync(widgetPushItem, model);

                await SaveStoreMappingsAsync(widgetPushItem, model);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.WidgetPush.WidgetPushItems.Updated"));

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = widgetPushItem.Id });
            }

            model = await _widgetPushItemModelFactory.PrepareWidgetPushItemModelAsync(model, widgetPushItem);
            return View(model);
        }

        [EditAccess, HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(WidgetPushPermissionProvider.ManageWidgetPush))
                return AccessDeniedView();

            var widgetPushItem = await _widgetPushItemService.GetWidgetPushItemByIdAsync(id);
            if (widgetPushItem == null)
                return RedirectToAction("List");

            await _widgetPushItemService.DeleteWidgetPushItemAsync(widgetPushItem);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.WidgetPush.WidgetPushItems.Deleted"));

            return RedirectToAction("List");
        }

        #endregion

        #endregion
    }
}
