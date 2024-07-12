using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Misc.WebApi.Extensions;
using NopStation.Plugin.Misc.WebApi.Services;
using NopStation.Plugin.Misc.WebApiNotification.Areas.Admin.Factories;
using NopStation.Plugin.Misc.WebApiNotification.Areas.Admin.Models;
using NopStation.Plugin.Misc.WebApiNotification.Services;

namespace NopStation.Plugin.Misc.WebApiNotification.Areas.Admin.Controllers
{
    public class WebApiNotificationController : NopStationAdminController
    {
        #region Fields

        private readonly INotificationService _notificationService;
        private readonly IWebApiNotificationModelFactory _webApiNotificationModelFactory;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IPermissionService _permissionService;
        private readonly IApiDeviceService _apiDeviceService;
        private readonly IPushNotificationSender _pushNotificationSender;
        private readonly ICustomerService _customerService;

        #endregion

        #region Ctor

        public WebApiNotificationController(INotificationService notificationService,
            IWebApiNotificationModelFactory webApiNotificationModelFactory,
            ILocalizationService localizationService,
            ISettingService settingService,
            IStoreContext storeContext,
            IPermissionService permissionService,
            IApiDeviceService apiDeviceService,
            IPushNotificationSender pushNotificationSender,
            ICustomerService customerService)
        {
            _permissionService = permissionService;
            _notificationService = notificationService;
            _webApiNotificationModelFactory = webApiNotificationModelFactory;
            _localizationService = localizationService;
            _settingService = settingService;
            _storeContext = storeContext;
            _apiDeviceService = apiDeviceService;
            _pushNotificationSender = pushNotificationSender;
            _customerService = customerService;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(WebApiNotificationPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var model = await _webApiNotificationModelFactory.PrepareConfigurationModelAsync(new ConfigurationModel());
            return View("~/Plugins/NopStation.Plugin.Misc.WebApiNotification/Areas/Admin/Views/WebApiNotification/Configure.cshtml", model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(WebApiNotificationPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var appSettings = await _settingService.LoadSettingAsync<WebApiNotificationSettings>(storeScope);
            appSettings = model.ToSettings(appSettings);

            await _settingService.SaveSettingOverridablePerStoreAsync(appSettings, x => x.GoogleConsoleApiAccessKey, model.GoogleConsoleApiAccessKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(appSettings, x => x.ApplicationTypeId, model.ApplicationTypeId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(appSettings, x => x.PushKitClientSecret, model.PushKitClientSecret_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(appSettings, x => x.PushKitClientId, model.PushKitClientId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(appSettings, x => x.PushKitAppId, model.PushKitAppId_OverrideForStore, storeScope, false);

            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }

        [HttpPost]
        public async Task<IActionResult> SendTestNotification(int id)
        {
            if (!await _permissionService.AuthorizeAsync(WebApiNotificationPermissionProvider.ManageConfiguration) ||
                !await _permissionService.AuthorizeAsync(WebApiPermissionProvider.ManageDevice))
                return AccessDeniedView();

            var device = await _apiDeviceService.GetApiDeviceByIdAsync(id);
            if (device == null)
                return Json(new { Result = false });

            var title = string.Format(await _localizationService.GetResourceAsync("Admin.NopStation.WebApiNotification.TestNotification.Title"),
                await _localizationService.GetResourceAsync("Admin.NopStation.WebApiNotification.TestNotification.Guest"));
            var customer = await _customerService.GetCustomerByIdAsync(device.CustomerId);
            if (customer != null && await _customerService.IsRegisteredAsync(customer))
                title = string.Format(await _localizationService.GetResourceAsync("Admin.NopStation.WebApiNotification.TestNotification.Title"),
                    await _customerService.GetCustomerFullNameAsync(customer));

            var body = string.Format(await _localizationService.GetResourceAsync("Admin.NopStation.WebApiNotification.TestNotification.Body"),
                await _localizationService.GetLocalizedAsync(await _storeContext.GetCurrentStoreAsync(), x => x.Name));

            var result = await _pushNotificationSender.SendNotification(device.DeviceType, title, body, device.SubscriptionId);

            if (result)
            {
                return Json(new
                {
                    Result = true,
                    Message = await _localizationService.GetResourceAsync("Admin.NopStation.WebApiNotification.TestNotification.SentSuccessFully")
                });
            }
            else
            {
                return Json(new
                {
                    Result = false,
                    Message = await _localizationService.GetResourceAsync("Admin.NopStation.WebApiNotification.TestNotification.SentUnSuccessFul")
                });
            }
        }

        #endregion
    }
}
