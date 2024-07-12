using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Widgets.PinterestAnalytics.Models;
using NopStation.Plugin.Widgets.PinterestAnalytics.Services;

namespace NopStation.Plugin.Widgets.PinterestAnalytics.Controllers
{
    public class PinterestAnalyticsController : NopStationAdminController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IPinterestAnalyticsService _pinterestAnalyticsService;

        #endregion

        #region Ctor

        public PinterestAnalyticsController(ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IPinterestAnalyticsService pinterestAnalyticsService,
            IStoreContext storeContext)
        {
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _storeContext = storeContext;
            _pinterestAnalyticsService = pinterestAnalyticsService;
        }

        #endregion

        #region Utilities

        public IList<SelectListItem> PreparePublicWidgetZones()
        {
            return typeof(PublicWidgetZones)
                .GetProperties(BindingFlags.Public | BindingFlags.Static)
                .Where(property => property.PropertyType == typeof(string))
                .Select(property => property.GetValue(null) is string value ? new SelectListItem(value, value) : null)
                .Where(item => item != null)
                .ToList();
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWidgets))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var pinterestAnalyticsSettings = await _settingService.LoadSettingAsync<PinterestAnalyticsSettings>(storeScope);

            var model = new ConfigurationModel
            {
                PinterestId = pinterestAnalyticsSettings.PinterestId,
                TrackingScript = pinterestAnalyticsSettings.TrackingScript,
                Ad_Account_Id = pinterestAnalyticsSettings.AdAccountId,
                AccessToken = pinterestAnalyticsSettings.AccessToken,
                SaveLog = pinterestAnalyticsSettings.SaveLog,
                ActiveStoreScopeConfiguration = storeScope,
            };

            if (storeScope > 0)
            {
                model.PinterestId_OverrideForStore = await _settingService.SettingExistsAsync(pinterestAnalyticsSettings, x => x.PinterestId, storeScope);
                model.TrackingScript_OverrideForStore = await _settingService.SettingExistsAsync(pinterestAnalyticsSettings, x => x.TrackingScript, storeScope);
                model.Ad_Account_Id_OverrideForStore = await _settingService.SettingExistsAsync(pinterestAnalyticsSettings, x => x.AdAccountId, storeScope);
                model.AccessToken_OverrideForStore = await _settingService.SettingExistsAsync(pinterestAnalyticsSettings, x => x.AccessToken, storeScope);
                model.SaveLog_OverrideForStore = await _settingService.SettingExistsAsync(pinterestAnalyticsSettings, x => x.SaveLog, storeScope);
            }
            model.HideCustomEventsSearch = (string.IsNullOrEmpty((await _settingService.LoadSettingAsync<PinterestAnalyticsSettings>(storeScope)).CustomEvents));
            model.CustomEventSearchModel.AddCustomEvent.AvailableWidgetZones = PreparePublicWidgetZones();
            model.CustomEventSearchModel.SetGridPageSize();
            return View(model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWidgets))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var pinterestAnalyticsSettings = await _settingService.LoadSettingAsync<PinterestAnalyticsSettings>(storeScope);
            if (pinterestAnalyticsSettings == null)
                return await Configure();

            pinterestAnalyticsSettings.PinterestId = model.PinterestId;
            pinterestAnalyticsSettings.TrackingScript = model.TrackingScript;
            pinterestAnalyticsSettings.AccessToken = model.AccessToken;
            pinterestAnalyticsSettings.AdAccountId = model.Ad_Account_Id;
            pinterestAnalyticsSettings.SaveLog = model.SaveLog;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            await _settingService.SaveSettingOverridablePerStoreAsync(pinterestAnalyticsSettings, x => x.PinterestId, model.PinterestId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(pinterestAnalyticsSettings, x => x.TrackingScript, model.TrackingScript_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(pinterestAnalyticsSettings, x => x.AccessToken, model.AccessToken_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(pinterestAnalyticsSettings, x => x.AdAccountId, model.Ad_Account_Id_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(pinterestAnalyticsSettings, x => x.SaveLog, model.SaveLog_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return await Configure();
        }

        [HttpPost]
        public virtual async Task<IActionResult> CustomEventList(CustomEventSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWidgets))
                return await AccessDeniedDataTablesJson();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var pinterestAnalyticsSettings = await _settingService.LoadSettingAsync<PinterestAnalyticsSettings>(storeScope)
                ?? throw new ArgumentException("No configuration found with the current store");

            var customEvents = (await _pinterestAnalyticsService.GetCustomEventsAsync(searchModel.WidgetZone)).ToPagedList(searchModel);
            var model = new CustomEventListModel().PrepareToGrid(searchModel, customEvents, () =>
            {
                //fill in model values from the configuration
                return customEvents.Select(customEvent => new CustomEventModel
                {
                    EventName = customEvent.EventName,
                    WidgetZonesName = string.Join(", ", customEvent.WidgetZones)
                });
            });

            return Json(model);
        }

        [EditAccess, HttpPost]
        public virtual async Task<IActionResult> CustomEventDelete(string id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWidgets))
                return AccessDeniedView();

            //save custom events configuration
            var eventName = id;
            await _pinterestAnalyticsService.SaveCustomEventsAsync(eventName, null);
            return new NullJsonResult();
        }

        [EditAccess, HttpPost]
        public virtual async Task<IActionResult> CustomEventAdd(int configurationId, [Validate] CustomEventModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageWidgets))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return ErrorJson(ModelState.SerializeErrors());

            //save custom events configuration
            await _pinterestAnalyticsService.SaveCustomEventsAsync(model.EventName, model.WidgetZones);

            return Json(new { Result = true });
        }

        #endregion
    }
}