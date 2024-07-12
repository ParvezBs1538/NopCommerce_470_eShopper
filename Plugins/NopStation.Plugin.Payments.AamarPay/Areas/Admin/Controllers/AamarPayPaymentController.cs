using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Payments.AamarPay.Areas.Admin.Models;

namespace NopStation.Plugin.Payments.AamarPay.Areas.Admin.Controllers;

public class AamarPayPaymentController : NopStationAdminController
{
    #region Fields

    private readonly IPermissionService _permissionService;
    private readonly IStoreContext _storeContext;
    private readonly ISettingService _settingService;
    private readonly INotificationService _notificationService;
    private readonly ILocalizationService _localizationService;

    #endregion

    #region Ctor

    public AamarPayPaymentController(IPermissionService permissionService,
        IStoreContext storeContext,
        ISettingService settingService,
        INotificationService notificationService,
        ILocalizationService localizationService)
    {
        _permissionService = permissionService;
        _storeContext = storeContext;
        _settingService = settingService;
        _notificationService = notificationService;
        _localizationService = localizationService;
    }

    #endregion

    #region Methods

    public async Task<IActionResult> Configure()
    {
        if (!await _permissionService.AuthorizeAsync(AamarPayPaymentPermissionProvider.ManageConfiguration))
            return AccessDeniedView();

        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var aamarPayPaymentSettings = await _settingService.LoadSettingAsync<AamarPayPaymentSettings>(storeScope);

        var model = new ConfigurationModel
        {
            UseSandbox = aamarPayPaymentSettings.UseSandbox,
            MerchantId = aamarPayPaymentSettings.MerchantId,
            SignatureKey = aamarPayPaymentSettings.SignatureKey,
            AdditionalFee = aamarPayPaymentSettings.AdditionalFee,
            AdditionalFeePercentage = aamarPayPaymentSettings.AdditionalFeePercentage,
        };

        if (storeScope > 0)
        {
            model.UseSandbox_OverrideForStore = await _settingService.SettingExistsAsync(aamarPayPaymentSettings, x => x.UseSandbox, storeScope);
            model.MerchantId_OverrideForStore = await _settingService.SettingExistsAsync(aamarPayPaymentSettings, x => x.MerchantId, storeScope);
            model.SignatureKey_OverrideForStore = await _settingService.SettingExistsAsync(aamarPayPaymentSettings, x => x.SignatureKey, storeScope);
            model.AdditionalFeePercentage_OverrideForStore = await _settingService.SettingExistsAsync(aamarPayPaymentSettings, x => x.AdditionalFeePercentage, storeScope);
            model.AdditionalFee_OverrideForStore = await _settingService.SettingExistsAsync(aamarPayPaymentSettings, x => x.AdditionalFee, storeScope);
        }

        model.ActiveStoreScopeConfiguration = storeScope;
        return View("~/Plugins/NopStation.Plugin.Payments.AamarPay/Areas/Admin/Views/AamarPayPayment/Configure.cshtml", model);
    }

    [EditAccess, HttpPost]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        if (!await _permissionService.AuthorizeAsync(AamarPayPaymentPermissionProvider.ManageConfiguration))
            return AccessDeniedView();

        if (!ModelState.IsValid)
            return RedirectToAction("Configure");

        var storeScope = model.ActiveStoreScopeConfiguration;

        var aamarPayPaymentSettings = await _settingService.LoadSettingAsync<AamarPayPaymentSettings>(storeScope);

        aamarPayPaymentSettings.UseSandbox = model.UseSandbox;
        aamarPayPaymentSettings.MerchantId = model.MerchantId;
        aamarPayPaymentSettings.SignatureKey = model.SignatureKey;
        aamarPayPaymentSettings.AdditionalFee = model.AdditionalFee;
        aamarPayPaymentSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;

        await _settingService.SaveSettingOverridablePerStoreAsync(aamarPayPaymentSettings, x => x.UseSandbox, model.UseSandbox_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(aamarPayPaymentSettings, x => x.MerchantId, model.MerchantId_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(aamarPayPaymentSettings, x => x.SignatureKey, model.SignatureKey_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(aamarPayPaymentSettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(aamarPayPaymentSettings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);

        await _settingService.ClearCacheAsync();

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.AamarPay.Configuration.Saved"));

        return RedirectToAction("Configure");
    }

    #endregion
}
