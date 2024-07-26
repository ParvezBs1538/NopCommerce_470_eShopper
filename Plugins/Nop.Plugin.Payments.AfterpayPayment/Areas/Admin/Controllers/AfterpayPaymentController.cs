using System;
using System.IO;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Plugin.Payments.AfterpayPayment.Areas.Admin.Models;
using Nop.Plugin.Payments.AfterpayPayment.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Controllers;

namespace Nop.Plugin.Payments.AfterpayPayment.Areas.Admin.Controllers
{
    public class AfterpayPaymentController : BaseAdminController
    {
        private readonly ILogger _logger;
        private readonly IPermissionService _permissionService;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;

        public AfterpayPaymentController(
            ILogger logger,
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
        public IActionResult Configure()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var afterpayPaymentSettings = _settingService.LoadSetting<AfterpayPaymentSettings>(storeScope);

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

            model.UseSandbox_OverrideForStore = _settingService.SettingExists(afterpayPaymentSettings, x => x.UseSandbox, storeScope);
            model.Debug_OverrideForStore = _settingService.SettingExists(afterpayPaymentSettings, x => x.Debug, storeScope);
            model.MerchantId_OverrideForStore = _settingService.SettingExists(afterpayPaymentSettings, x => x.MerchantId, storeScope);
            model.MerchantKey_OverrideForStore = _settingService.SettingExists(afterpayPaymentSettings, x => x.MerchantKey, storeScope);
            model.MaximumAmount_OverrideForStore = _settingService.SettingExists(afterpayPaymentSettings, x => x.MaximumAmount, storeScope);
            model.MinimumAmount_OverrideForStore = _settingService.SettingExists(afterpayPaymentSettings, x => x.MinimumAmount, storeScope);
            return View(model);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public IActionResult Configure(ConfigurationModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return Configure();

            var storeScope = model.ActiveStoreScopeConfiguration;
            //load settings for a chosen store scope
            var afterpayPaymentSettings = _settingService.LoadSetting<AfterpayPaymentSettings>(storeScope);

            afterpayPaymentSettings.UseSandbox = model.UseSandbox;
            afterpayPaymentSettings.Debug = model.Debug;
            afterpayPaymentSettings.MerchantId = model.MerchantId;
            afterpayPaymentSettings.MerchantKey = model.MerchantKey;


            var resourceAddress = AfterpayPaymentDefaults.CONFIGURATION;
            var baseUrl = afterpayPaymentSettings.UseSandbox ? AfterpayPaymentDefaults.SANDBOX_BASE_URL : AfterpayPaymentDefaults.BASE_URL;
            var request = WebRequest.Create($"{baseUrl}{resourceAddress}");
            var storeLocation = _webHelper.GetStoreLocation();
            request = AfterpayPaymentHelper.AddHeaders(request, "GET", afterpayPaymentSettings, storeLocation);

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
                _logger.Error(e.Message);
            }

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */


            _settingService.SaveSettingOverridablePerStore(afterpayPaymentSettings, x => x.UseSandbox, model.UseSandbox_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(afterpayPaymentSettings, x => x.Debug, model.Debug_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(afterpayPaymentSettings, x => x.MerchantId, model.MerchantId_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(afterpayPaymentSettings, x => x.MerchantKey, model.MerchantId_OverrideForStore, storeScope, false);

            _settingService.SaveSettingOverridablePerStore(afterpayPaymentSettings, x => x.MinimumAmount, model.MinimumAmount_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(afterpayPaymentSettings, x => x.MaximumAmount, model.MaximumAmount_OverrideForStore, storeScope, false);

            //now clear settings cache
            _settingService.ClearCache();

            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }
    }
}
