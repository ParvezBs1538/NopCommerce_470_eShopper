using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Payments.MPay24.Areas.Admin.Factories;
using NopStation.Plugin.Payments.MPay24.Areas.Admin.Models;
using NopStation.Plugin.Payments.MPay24.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using NopStation.Plugin.Payments.MPay24.Domains;

namespace NopStation.Plugin.Payments.MPay24.Areas.Admin.Controllers
{
    public class PaymentMPay24Controller : NopStationAdminController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly INotificationService _notificationService;
        private readonly IPaymentOptionService _paymentOptionService;
        private readonly IPaymentOptionModelFactory _paymentOptionModelFactory;

        #endregion

        #region Ctor

        public PaymentMPay24Controller(ILocalizationService localizationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IStoreContext storeContext,
            INotificationService notificationService,
            IPaymentOptionService paymentOptionService,
            IPaymentOptionModelFactory paymentOptionModelFactory)
        {
            _localizationService = localizationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _storeContext = storeContext;
            _notificationService = notificationService;
            _paymentOptionService = paymentOptionService;
            _paymentOptionModelFactory = paymentOptionModelFactory;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(MPay24PaymentPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var mPay24PaymentSettings = await _settingService.LoadSettingAsync<MPay24PaymentSettings>(storeScope);

            var model = new ConfigurationModel
            {
                Sandbox = mPay24PaymentSettings.Sandbox,
                SoapUsername = mPay24PaymentSettings.SoapUsername,
                SoapPassword = mPay24PaymentSettings.SoapPassword,
                AdditionalFeePercentage = mPay24PaymentSettings.AdditionalFeePercentage,
                AdditionalFee = mPay24PaymentSettings.AdditionalFee,
                MerchantId = mPay24PaymentSettings.MerchantId,
                ActiveStoreScopeConfiguration = storeScope
            };

            if (storeScope > 0)
            {
                model.Sandbox_OverrideForStore = await _settingService.SettingExistsAsync(mPay24PaymentSettings, x => x.Sandbox, storeScope);
                model.SoapUsername_OverrideForStore = await _settingService.SettingExistsAsync(mPay24PaymentSettings, x => x.SoapUsername, storeScope);
                model.SoapPassword_OverrideForStore = await _settingService.SettingExistsAsync(mPay24PaymentSettings, x => x.SoapPassword, storeScope);
                model.AdditionalFeePercentage_OverrideForStore = await _settingService.SettingExistsAsync(mPay24PaymentSettings, x => x.AdditionalFeePercentage, storeScope);
                model.AdditionalFee_OverrideForStore = await _settingService.SettingExistsAsync(mPay24PaymentSettings, x => x.AdditionalFee, storeScope);
                model.MerchantId_OverrideForStore = await _settingService.SettingExistsAsync(mPay24PaymentSettings, x => x.MerchantId, storeScope);
            }

            return View(model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(MPay24PaymentPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return RedirectToAction("Configure");

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var mPay24PaymentSettings = await _settingService.LoadSettingAsync<MPay24PaymentSettings>(storeScope);

            //save settings
            mPay24PaymentSettings.Sandbox = model.Sandbox;
            mPay24PaymentSettings.SoapUsername = model.SoapUsername;
            mPay24PaymentSettings.SoapPassword = model.SoapPassword;
            mPay24PaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;
            mPay24PaymentSettings.AdditionalFee = model.AdditionalFee;
            mPay24PaymentSettings.MerchantId = model.MerchantId;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            await _settingService.SaveSettingOverridablePerStoreAsync(mPay24PaymentSettings, x => x.Sandbox, model.Sandbox_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(mPay24PaymentSettings, x => x.SoapUsername, model.SoapUsername_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(mPay24PaymentSettings, x => x.SoapPassword, model.SoapPassword_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(mPay24PaymentSettings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(mPay24PaymentSettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(mPay24PaymentSettings, x => x.MerchantId, model.MerchantId_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Configure");
        }

        public async Task<IActionResult> PaymentOptions()
        {
            if (!await _permissionService.AuthorizeAsync(MPay24PaymentPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var model = await _paymentOptionModelFactory.PreparePaymentOptionSearchModelAsync(new PaymentOptionSearchModel());
            return View(model);
        }

        [EditAccessAjax, HttpPost]
        public async Task<IActionResult> PaymentOptions(PaymentOptionSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(MPay24PaymentPermissionProvider.ManageConfiguration))
                return await AccessDeniedDataTablesJson();

            var paymentOptions = await _paymentOptionModelFactory.PreparePaymentOptionListModelAsync(searchModel);
            return Json(paymentOptions);
        }

        public async Task<IActionResult> PaymentOptionCreate()
        {
            if (!await _permissionService.AuthorizeAsync(MPay24PaymentPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var model = await _paymentOptionModelFactory.PreparePaymentOptionModelAsync(new PaymentOptionModel(), null);
            return View(model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> PaymentOptionCreate(PaymentOptionModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(MPay24PaymentPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var oldOption = await _paymentOptionService.GetPaymentOptionByShortNameAsync(model.ShortName);
            if (oldOption != null)
                ModelState.AddModelError("", await _localizationService.GetResourceAsync("Admin.NopStation.MPay24.PaymentOptions.ShortNameAlreadyExists"));

            if (ModelState.IsValid)
            {
                var option = model.ToEntity<PaymentOption>();
                await _paymentOptionService.InsertPaymentOptionAsync(option);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.MPay24.PaymentOptions.Created"));

                return continueEditing
                    ? RedirectToAction("PaymentOptionEdit", new { id = option.Id })
                    : RedirectToAction("PaymentOptions");
            }

            return RedirectToAction("PaymentOptions");
        }

        public async Task<IActionResult> PaymentOptionEdit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(MPay24PaymentPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var option = await _paymentOptionService.GetPaymentOptionByIdAsync(id);
            if (option == null)
                return RedirectToAction("PaymentOptions");

            var model = await _paymentOptionModelFactory.PreparePaymentOptionModelAsync(null, option);
            return View(model);
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> PaymentOptionEdit(PaymentOptionModel model, bool continueEditing)
        {
            if (!await _permissionService.AuthorizeAsync(MPay24PaymentPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var option = await _paymentOptionService.GetPaymentOptionByIdAsync(model.Id);
            if (option == null)
                return RedirectToAction("PaymentOptions");

            var oldOption = await _paymentOptionService.GetPaymentOptionByShortNameAsync(model.ShortName);
            if (oldOption != null && option.Id != oldOption.Id)
                ModelState.AddModelError("", await _localizationService.GetResourceAsync("Admin.NopStation.MPay24.PaymentOptions.ShortNameAlreadyExists"));

            if (ModelState.IsValid)
            {
                option = model.ToEntity(option);
                await _paymentOptionService.UpdatePaymentOptionAsync(option);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.MPay24.PaymentOptions.Updated"));

                return continueEditing
                    ? RedirectToAction("PaymentOptionEdit", new { id = option.Id })
                    : RedirectToAction("PaymentOptions");
            }

            return RedirectToAction("PaymentOptions");
        }

        [EditAccess, HttpPost]
        public async Task<IActionResult> PaymentOptionDelete(PaymentOptionModel model)
        {
            if (!await _permissionService.AuthorizeAsync(MPay24PaymentPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var option = await _paymentOptionService.GetPaymentOptionByIdAsync(model.Id);
            if (option == null)
                return RedirectToAction("PaymentOptions");

            await _paymentOptionService.DeletePaymentOptionAsync(option);
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.MPay24.PaymentOptions.Deleted"));
            
            return RedirectToAction("PaymentOptions");
        }

        #endregion
    }
}
