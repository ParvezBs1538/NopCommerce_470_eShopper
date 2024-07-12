using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Security;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Payments.Afterpay.Areas.Admin.Models;
using NopStation.Plugin.Payments.Afterpay.Models;

namespace NopStation.Plugin.Payments.Afterpay.Areas.Admin.Controllers
{
    public class AfterpayPaymentController : NopStationAdminController
    {
        private readonly ILogger _logger;
        private readonly IPermissionService _permissionService;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;

        public AfterpayPaymentController(ILogger logger,
            IPermissionService permissionService,
            IStoreContext storeContext,
            ISettingService settingService,
            INotificationService notificationService,
            ILocalizationService localizationService,
            IWebHelper webHelper)
        {
            _logger = logger;
            _permissionService = permissionService;
            _storeContext = storeContext;
            _settingService = settingService;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _webHelper = webHelper;
        }

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(AfterpayPaymentPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var afterpayPaymentSettings = await _settingService.LoadSettingAsync<AfterpayPaymentSettings>(storeScope);

            var model = new ConfigurationModel
            {
                ActiveStoreScopeConfiguration = storeScope,
                Debug = afterpayPaymentSettings.Debug,
                UseSandbox = afterpayPaymentSettings.UseSandbox,
                MerchantId = afterpayPaymentSettings.MerchantId,
                MerchantKey = afterpayPaymentSettings.MerchantKey,
                MinimumAmount = afterpayPaymentSettings.MinimumAmount,
                MaximumAmount = afterpayPaymentSettings.MaximumAmount
            };

            if (storeScope <= 0)
                return View(model);

            model.UseSandbox_OverrideForStore = await _settingService.SettingExistsAsync(afterpayPaymentSettings, x => x.UseSandbox, storeScope);
            model.Debug_OverrideForStore = await _settingService.SettingExistsAsync(afterpayPaymentSettings, x => x.Debug, storeScope);
            model.MerchantId_OverrideForStore = await _settingService.SettingExistsAsync(afterpayPaymentSettings, x => x.MerchantId, storeScope);
            model.MerchantKey_OverrideForStore = await _settingService.SettingExistsAsync(afterpayPaymentSettings, x => x.MerchantKey, storeScope);
            model.MinimumAmount_OverrideForStore = await _settingService.SettingExistsAsync(afterpayPaymentSettings, x => x.MinimumAmount, storeScope);
            model.MaximumAmount_OverrideForStore = await _settingService.SettingExistsAsync(afterpayPaymentSettings, x => x.MaximumAmount, storeScope);

            return View(model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(AfterpayPaymentPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return RedirectToAction("Configure");

            var storeScope = model.ActiveStoreScopeConfiguration;
            //load settings for a chosen store scope
            var afterpayPaymentSettings = await _settingService.LoadSettingAsync<AfterpayPaymentSettings>(storeScope);

            afterpayPaymentSettings.UseSandbox = model.UseSandbox;
            afterpayPaymentSettings.Debug = model.Debug;
            afterpayPaymentSettings.MerchantId = model.MerchantId;
            afterpayPaymentSettings.MerchantKey = model.MerchantKey;

            var resourceAddress = AfterpayPaymentDefaults.CONFIGURATION;
            var baseUrl = afterpayPaymentSettings.UseSandbox ? AfterpayPaymentDefaults.SANDBOX_BASE_URL : AfterpayPaymentDefaults.BASE_URL;
            var request = WebRequest.Create($"{baseUrl}{resourceAddress}");
            var storeLocation = _webHelper.GetStoreLocation();
            request = request.AddHeaders("GET", afterpayPaymentSettings, storeLocation);

            try
            {
                var webResponse = request.GetResponse();
                using var webStream = webResponse.GetResponseStream() ?? Stream.Null;
                using var responseReader = new StreamReader(webStream);
                var response = responseReader.ReadToEnd();
                var configurationResponse = JsonConvert.DeserializeObject<ConfigurationResponse>(response);
                var minAmount = configurationResponse.MinimumAmount != null ? Convert.ToInt32(configurationResponse.MinimumAmount.PaymentAmount) : 1;
                var maxAmount = configurationResponse.MaximumAmount != null ? (int)float.Parse(configurationResponse.MaximumAmount.PaymentAmount) : 1000;

                afterpayPaymentSettings.MaximumAmount = maxAmount;
                afterpayPaymentSettings.MinimumAmount = minAmount;
            }
            catch (Exception e)
            {
                await _logger.ErrorAsync(e.Message);
                _notificationService.ErrorNotification(e.Message);
            }

            await _settingService.SaveSettingOverridablePerStoreAsync(afterpayPaymentSettings, x => x.UseSandbox, model.UseSandbox_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(afterpayPaymentSettings, x => x.Debug, model.Debug_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(afterpayPaymentSettings, x => x.MerchantId, model.MerchantId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(afterpayPaymentSettings, x => x.MerchantKey, model.MerchantKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(afterpayPaymentSettings, x => x.MinimumAmount, false, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(afterpayPaymentSettings, x => x.MaximumAmount, false, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.Afterpay.Configuration.Saved"));

            return RedirectToAction("Configure");
        }
    }
}
