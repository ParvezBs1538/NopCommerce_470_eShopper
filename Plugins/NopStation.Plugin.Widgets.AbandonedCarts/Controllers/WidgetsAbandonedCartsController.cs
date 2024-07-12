using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Security;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Widgets.AbandonedCarts.Domain;
using NopStation.Plugin.Widgets.AbandonedCarts.Factories;
using NopStation.Plugin.Widgets.AbandonedCarts.Models;
using NopStation.Plugin.Widgets.AbandonedCarts.Services;

namespace NopStation.Plugin.Widgets.AbandonedCarts.Controllers
{
    public class WidgetsAbandonedCartsController : NopStationAdminController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IAbandonedCartFactory _abandonedCartFactory;
        private readonly IAbandonedCartService _abandonedCartService;
        private readonly ILogger _logger;

        #endregion

        #region Ctor

        public WidgetsAbandonedCartsController(ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IStoreContext storeContext,
            IAbandonedCartFactory abandonedCartFactory,
            ILogger logger,
            IAbandonedCartService abandonedCartService)
        {
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _storeContext = storeContext;
            _abandonedCartFactory = abandonedCartFactory;
            _logger = logger;
            _abandonedCartService = abandonedCartService;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var abandonedCartsSettings = await _settingService.LoadSettingAsync<AbandonmentCartSettings>(storeScope);

            var model = new ConfigurationModel
            {
                IsEnabled = abandonedCartsSettings.IsEnabled,
                AbandonmentCutOffTime = abandonedCartsSettings.AbandonmentCutOffTime,
                NotificationSendingConditionId = (int)abandonedCartsSettings.NotificationSendingConditionId,
                IsEnabledFirstNotification = abandonedCartsSettings.IsEnabledFirstNotification,
                DurationAfterFirstAbandonment = abandonedCartsSettings.DurationAfterFirstAbandonment,
                IsEnabledSecondNotification = abandonedCartsSettings.IsEnabledSecondNotification,
                DurationAfterSecondAbandonment = abandonedCartsSettings.DurationAfterSecondAbandonment,
                ActiveStoreScopeConfiguration = storeScope,
                DontSendSameDay = abandonedCartsSettings.DontSendSameDay,
                CustomerOnlineCutoffTime = abandonedCartsSettings.CustomerOnlineCutoffTime
            };

            if (storeScope > 0)
            {
                model.IsEnabled_OverrideForStore = await _settingService.SettingExistsAsync(abandonedCartsSettings, x => x.IsEnabled, storeScope);
                model.AbandonmentCutOffTime_OverrideForStore = await _settingService.SettingExistsAsync(abandonedCartsSettings, x => x.AbandonmentCutOffTime, storeScope);
                model.NotificationSendingCondition_OverrideForStore = await _settingService.SettingExistsAsync(abandonedCartsSettings, x => x.NotificationSendingConditionId, storeScope);
                model.IsEnabledFirstNotification_OverrideForStore = await _settingService.SettingExistsAsync(abandonedCartsSettings, x => x.IsEnabledFirstNotification, storeScope);
                model.DurationAfterFirstAbandonment_OverrideForStore = await _settingService.SettingExistsAsync(abandonedCartsSettings, x => x.DurationAfterFirstAbandonment, storeScope);
                model.IsEnabledSecondNotification_OverrideForStore = await _settingService.SettingExistsAsync(abandonedCartsSettings, x => x.IsEnabledSecondNotification, storeScope);
                model.DurationAfterSecondAbandonment_OverrideForStore = await _settingService.SettingExistsAsync(abandonedCartsSettings, x => x.DurationAfterSecondAbandonment, storeScope);
                model.DontSendSameDay_OverrideForStore = await _settingService.SettingExistsAsync(abandonedCartsSettings, x => x.DontSendSameDay, storeScope);
                model.CustomerOnlineCutoffTime_OverrideForStore = await _settingService.SettingExistsAsync(abandonedCartsSettings, x => x.CustomerOnlineCutoffTime, storeScope);
            }

            return View(model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return await Configure();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var abandonedCartsSettings = await _settingService.LoadSettingAsync<AbandonmentCartSettings>(storeScope);

            //save settings
            abandonedCartsSettings.IsEnabled = model.IsEnabled;
            abandonedCartsSettings.AbandonmentCutOffTime = model.AbandonmentCutOffTime;
            abandonedCartsSettings.NotificationSendingConditionId = (AbandonedType)model.NotificationSendingConditionId;
            abandonedCartsSettings.IsEnabledFirstNotification = model.IsEnabledFirstNotification;
            abandonedCartsSettings.DurationAfterFirstAbandonment = model.DurationAfterFirstAbandonment;
            abandonedCartsSettings.IsEnabledSecondNotification = model.IsEnabledSecondNotification;
            abandonedCartsSettings.DurationAfterSecondAbandonment = model.DurationAfterSecondAbandonment;
            abandonedCartsSettings.DontSendSameDay = model.DontSendSameDay;
            abandonedCartsSettings.CustomerOnlineCutoffTime = model.CustomerOnlineCutoffTime;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            await _settingService.SaveSettingOverridablePerStoreAsync(abandonedCartsSettings, x => x.IsEnabled, model.IsEnabled_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(abandonedCartsSettings, x => x.AbandonmentCutOffTime, model.AbandonmentCutOffTime_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(abandonedCartsSettings, x => x.NotificationSendingConditionId, model.NotificationSendingCondition_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(abandonedCartsSettings, x => x.IsEnabledFirstNotification, model.IsEnabledFirstNotification_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(abandonedCartsSettings, x => x.DurationAfterFirstAbandonment, model.DurationAfterFirstAbandonment_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(abandonedCartsSettings, x => x.IsEnabledSecondNotification, model.IsEnabledSecondNotification_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(abandonedCartsSettings, x => x.DurationAfterSecondAbandonment, model.DurationAfterSecondAbandonment_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(abandonedCartsSettings, x => x.DontSendSameDay, model.DontSendSameDay_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(abandonedCartsSettings, x => x.CustomerOnlineCutoffTime, model.CustomerOnlineCutoffTime_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return await Configure();
        }

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            //prepare model
            var model = _abandonedCartFactory.PrepareAbandonedCartSearchModel(new AbandonedCartSearchModel());

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(AbandonedCartSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await _abandonedCartFactory.PrepareCustomerAbandonmentListModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> ListDetails(AbandonedCartSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await _abandonedCartFactory.PrepareAbandonedCartsListModelAsync(searchModel);

            return Json(model);
        }

        public virtual async Task<IActionResult> Edit(int id)
        {
            try
            {
                if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
                    return AccessDeniedView();

                //prepare model
                var model = await _abandonedCartFactory.PrepareAbandonedCartDetailViewModel(id);

                if (model == null)
                    return RedirectToAction("List");

                return View(model);
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync("(AbandonedCartPlugin) Exception occurred while getting Abandonment Details", ex);
                return RedirectToAction("List");
            }
        }

        [EditAccess, HttpPost]
        public virtual async Task<IActionResult> DeleteAbandonedCarts(AbandonmentMaintenanceModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            model.NumberOfDeletedItems = await _abandonedCartService.BulkDeleteAbandonedCartsAsync(model);

            return Json(model);
        }

        #endregion
    }
}