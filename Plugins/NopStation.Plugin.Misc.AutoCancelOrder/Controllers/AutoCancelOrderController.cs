using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using NopStation.Plugin.Misc.AutoCancelOrder.Models;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using Nop.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;

namespace NopStation.Plugin.Misc.AutoCancelOrder.Controllers
{
    public class AutoCancelOrderController : NopStationAdminController
    {
        #region Fields

        private readonly IStoreContext _storeContext;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;

        #endregion

        #region Ctor

        public AutoCancelOrderController(IStoreContext storeContext,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IPaymentPluginManager paymentPluginManager,
            IBaseAdminModelFactory baseAdminModelFactory)
        {
            _storeContext = storeContext;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _paymentPluginManager = paymentPluginManager;
            _baseAdminModelFactory = baseAdminModelFactory;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(AutoCancelOrderPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var autoCancelOrderSettings = await _settingService.LoadSettingAsync<AutoCancelOrderSettings>(storeId);

            var model = new ConfigurationModel()
            { 
                ApplyOnOrderStatuses = autoCancelOrderSettings.ApplyOnOrderStatuses,
                EnablePlugin = autoCancelOrderSettings.EnablePlugin,
                ApplyOnPaymentMethods = autoCancelOrderSettings.ApplyOnPaymentMethods,
                SendNotificationToCustomer = autoCancelOrderSettings.SendNotificationToCustomer,
                ApplyOnShippingStatuses = autoCancelOrderSettings.ApplyOnShippingStatuses,
                ActiveStoreScopeConfiguration = storeId
            };

            model.AvailablePaymentMethods = (await _paymentPluginManager.LoadAllPluginsAsync()).Select(method =>
                new SelectListItem { Text = method.PluginDescriptor.FriendlyName, Value = method.PluginDescriptor.SystemName }).ToList();

            await _baseAdminModelFactory.PrepareOrderStatusesAsync(model.AvailableOrderStatuses, false);
            await _baseAdminModelFactory.PrepareShippingStatusesAsync(model.AvailableShippingStatuses, false);
            model.AvailableUnits = (await UnitType.Days.ToSelectListAsync()).ToList();

            if (storeId > 0)
            {
                model.EnablePlugin_OverrideForStore = await _settingService.SettingExistsAsync(autoCancelOrderSettings, x => x.EnablePlugin, storeId);
                model.SendNotificationToCustomer_OverrideForStore = await _settingService.SettingExistsAsync(autoCancelOrderSettings, x => x.SendNotificationToCustomer, storeId);
                model.ApplyOnShippingStatuses_OverrideForStore = await _settingService.SettingExistsAsync(autoCancelOrderSettings, x => x.ApplyOnShippingStatuses, storeId);
                model.ApplyOnOrderStatuses_OverrideForStore = await _settingService.SettingExistsAsync(autoCancelOrderSettings, x => x.ApplyOnOrderStatuses, storeId);
                model.ApplyOnPaymentMethods_OverrideForStore = await _settingService.SettingExistsAsync(autoCancelOrderSettings, x => x.SerializedApplyOnPaymentMethods, storeId);
            }

            return View("~/Plugins/NopStation.Plugin.Misc.AutoCancelOrder/Views/AutoCancelOrder/Configure.cshtml", model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(AutoCancelOrderPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var autoCancelOrderSettings = await _settingService.LoadSettingAsync<AutoCancelOrderSettings>(storeScope);

            autoCancelOrderSettings.ApplyOnOrderStatuses = model.ApplyOnOrderStatuses.ToList();
            autoCancelOrderSettings.EnablePlugin = model.EnablePlugin;
            autoCancelOrderSettings.ApplyOnShippingStatuses = model.ApplyOnShippingStatuses.ToList();
            autoCancelOrderSettings.SendNotificationToCustomer = model.SendNotificationToCustomer;

            try
            {
                autoCancelOrderSettings.ApplyOnPaymentMethods = GetApplyOnPaymentMethods(model);
            }
            catch
            {
                _notificationService.WarningNotification(await _localizationService.GetResourceAsync("Admin.NopStation.AutoCancelOrder.Configuration.Offset.Error"));
                return RedirectToAction("Configure");
            }

            await _settingService.SaveSettingOverridablePerStoreAsync(autoCancelOrderSettings, x => x.EnablePlugin, model.EnablePlugin_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(autoCancelOrderSettings, x => x.SendNotificationToCustomer, model.SendNotificationToCustomer_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(autoCancelOrderSettings, x => x.ApplyOnShippingStatuses, model.ApplyOnShippingStatuses_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(autoCancelOrderSettings, x => x.ApplyOnOrderStatuses, model.ApplyOnOrderStatuses_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(autoCancelOrderSettings, x => x.SerializedApplyOnPaymentMethods, model.ApplyOnPaymentMethods_OverrideForStore, storeScope, false);

            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }

        protected IList<PaymentMethodOffset> GetApplyOnPaymentMethods(ConfigurationModel model)
        {
            var list = new List<PaymentMethodOffset>();
            if (model.UnitTypeId == null || model.UnitTypeId.Length == 0)
                return list;

            for (var i = 0; i < model.UnitTypeId.Length; i++)
            {
                if (list.Any(x => x.SystemName == model.SystemName[i]))
                    continue;

                list.Add(new PaymentMethodOffset()
                {
                    Offset = model.Offset[i],
                    SystemName = model.SystemName[i],
                    UnitTypeId = model.UnitTypeId[i]
                });
            }

            return list;
        }

        #endregion
    }
}
