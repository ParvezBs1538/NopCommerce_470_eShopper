using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using NopStation.Plugin.Widgets.CancelOrder.Areas.Admin.Models;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using System.Linq;
using System.Threading.Tasks;

namespace NopStation.Plugin.Widgets.CancelOrder.Areas.Admin.Controllers
{
    public class CancelOrderController : NopStationAdminController
    {
        private readonly ISettingService _settingService;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly IStoreContext _storeContext;

        public CancelOrderController(ISettingService settingService,
            IBaseAdminModelFactory baseAdminModelFactory,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            IStoreContext storeContext)
        {
            _settingService = settingService;
            _baseAdminModelFactory = baseAdminModelFactory;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _storeContext = storeContext;
        }

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(CancelOrderPermissionProvider.ManageCancelOrder))
                return AccessDeniedView();

            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var cancelOrderSettings = await _settingService.LoadSettingAsync<CancelOrderSettings>(storeId);

            var model = new ConfigurationModel()
            {
                CancellableOrderStatuses = cancelOrderSettings.CancellableOrderStatuses,
                CancellablePaymentStatuses = cancelOrderSettings.CancellablePaymentStatuses,
                CancellableShippingStatuses = cancelOrderSettings.CancellableShippingStatuses,
                WidgetZone = cancelOrderSettings.WidgetZone
            };

            await _baseAdminModelFactory.PrepareOrderStatusesAsync(model.AvailableOrderStatuses, false);
            await _baseAdminModelFactory.PreparePaymentStatusesAsync(model.AvailablePaymentStatuses, false);
            await _baseAdminModelFactory.PrepareShippingStatusesAsync(model.AvailableShippingStatuses, false);

            model.ActiveStoreScopeConfiguration = storeId;

            if (storeId <= 0)
                return View(model);

            model.WidgetZone_OverrideForStore = await _settingService.SettingExistsAsync(cancelOrderSettings, x => x.WidgetZone, storeId);
            model.CancellableOrderStatuses_OverrideForStore = await _settingService.SettingExistsAsync(cancelOrderSettings, x => x.CancellableOrderStatuses, storeId);
            model.CancellablePaymentStatuses_OverrideForStore = await _settingService.SettingExistsAsync(cancelOrderSettings, x => x.CancellablePaymentStatuses, storeId);
            model.CancellableShippingStatuses_OverrideForStore = await _settingService.SettingExistsAsync(cancelOrderSettings, x => x.CancellableShippingStatuses, storeId);

            return View(model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(CancelOrderPermissionProvider.ManageCancelOrder))
                return AccessDeniedView();

            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var cancelOrderSettings = await _settingService.LoadSettingAsync<CancelOrderSettings>(storeId);

            cancelOrderSettings.CancellableOrderStatuses = model.CancellableOrderStatuses.ToList();
            cancelOrderSettings.CancellablePaymentStatuses = model.CancellablePaymentStatuses.ToList();
            cancelOrderSettings.CancellableShippingStatuses = model.CancellableShippingStatuses.ToList();
            cancelOrderSettings.WidgetZone = model.WidgetZone;

            await _settingService.SaveSettingOverridablePerStoreAsync(cancelOrderSettings, x => x.CancellableOrderStatuses, model.CancellableOrderStatuses_OverrideForStore, storeId, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(cancelOrderSettings, x => x.CancellablePaymentStatuses, model.CancellablePaymentStatuses_OverrideForStore, storeId, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(cancelOrderSettings, x => x.CancellableShippingStatuses, model.CancellableShippingStatuses_OverrideForStore, storeId, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(cancelOrderSettings, x => x.WidgetZone, model.WidgetZone_OverrideForStore, storeId, false);

            await _settingService.ClearCacheAsync();
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }
    }
}
