using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Payments.StripePaymentElement.Areas.Admin.Models;

namespace NopStation.Plugin.Payments.StripePaymentElement.Areas.Admin.Controllers;

public class StripePaymentElementController : NopStationAdminController
{
    #region Field  

    private readonly ISettingService _settingService;
    private readonly ILocalizationService _localizationService;
    private readonly IStoreContext _storeContext;
    private readonly INotificationService _notificationService;
    private readonly IPermissionService _permissionService;
    private readonly INopFileProvider _nopFileProvider;

    #endregion

    #region Ctor

    public StripePaymentElementController(
        ISettingService settingService,
        ILocalizationService localizationService,
        IStoreContext storeContext,
        INotificationService notificationService,
        IPermissionService permissionService,
        INopFileProvider nopFileProvider)
    {
        _settingService = settingService;
        _localizationService = localizationService;
        _storeContext = storeContext;
        _notificationService = notificationService;
        _permissionService = permissionService;
        _nopFileProvider = nopFileProvider;
    }

    #endregion

    #region Methods

    public async Task<IActionResult> Configure()
    {
        if (!await _permissionService.AuthorizeAsync(StripePaymentElementsPermissionProvider.ManageConfiguration))
            return AccessDeniedView();

        //load settings for a chosen store scope
        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var stripePaymentElementSettings = await _settingService.LoadSettingAsync<StripePaymentElementSettings>(storeScope);

        var model = new ConfigurationModel
        {
            TransactionModeId = Convert.ToInt32(stripePaymentElementSettings.TransactionMode),
            AdditionalFee = stripePaymentElementSettings.AdditionalFee,
            AdditionalFeePercentage = stripePaymentElementSettings.AdditionalFeePercentage,
            TransactionModeValues = (await stripePaymentElementSettings.TransactionMode.ToSelectListAsync()).ToList(),
            ApiKey = stripePaymentElementSettings.SecretKey,
            ActiveStoreScopeConfiguration = storeScope,
            EnableLogging = stripePaymentElementSettings.EnableLogging,
            PublishableKey = stripePaymentElementSettings.PublishableKey,
            Theme = stripePaymentElementSettings.Theme,
            ThemeValues = StripeDefaults.Themes.Select(x => new SelectListItem(x, x)).ToList(),
            Layout = stripePaymentElementSettings.Layout,
            LayoutValues = StripeDefaults.Layouts.Select(x => new SelectListItem(x, x)).ToList(),
        };

        if (storeScope > 0)
        {
            model.TransactionModeId_OverrideForStore = await _settingService.SettingExistsAsync(stripePaymentElementSettings, x => x.TransactionMode, storeScope);
            model.AdditionalFee_OverrideForStore = await _settingService.SettingExistsAsync(stripePaymentElementSettings, x => x.AdditionalFee, storeScope);
            model.AdditionalFeePercentage_OverrideForStore = await _settingService.SettingExistsAsync(stripePaymentElementSettings, x => x.AdditionalFeePercentage, storeScope);
            model.ApiKey_OverrideForStore = await _settingService.SettingExistsAsync(stripePaymentElementSettings, x => x.SecretKey, storeScope);
            model.PublishableKey_OverrideForStore = await _settingService.SettingExistsAsync(stripePaymentElementSettings, x => x.PublishableKey, storeScope);
            model.Theme_OverrideForStore = await _settingService.SettingExistsAsync(stripePaymentElementSettings, x => x.Theme, storeScope);
            model.Layout_OverrideForStore = await _settingService.SettingExistsAsync(stripePaymentElementSettings, x => x.Layout, storeScope);
            model.EnableLogging_OverrideForStore = await _settingService.SettingExistsAsync(stripePaymentElementSettings, x => x.EnableLogging, storeScope);
        }

        var filePath = _nopFileProvider.MapPath(string.Format(StripeDefaults.AppleVerificationFilePath, storeScope));
        model.AppleVerificationFileExist = _nopFileProvider.FileExists(filePath);

        //if (!model.AppleVerificationFileExist)
        //    _notificationService.WarningNotification(
        //        await _localizationService.GetResourceAsync("Admin.NopStation.StripePaymentElement.Configuration.AppleVerificationFileNotExist"));

        return View("~/Plugins/NopStation.Plugin.Payments.StripePaymentElement/Areas/Admin/Views/StripePaymentElement/Configure.cshtml", model);
    }

    [EditAccess, HttpPost]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        if (!await _permissionService.AuthorizeAsync(StripePaymentElementsPermissionProvider.ManageConfiguration))
            return AccessDeniedView();

        //load settings for a chosen store scope
        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var stripePaymentElementSettings = await _settingService.LoadSettingAsync<StripePaymentElementSettings>(storeScope);

        //save settings
        stripePaymentElementSettings.TransactionMode = (TransactionMode)model.TransactionModeId;
        stripePaymentElementSettings.AdditionalFee = model.AdditionalFee;
        stripePaymentElementSettings.AdditionalFeePercentage = model.AdditionalFeePercentage;
        stripePaymentElementSettings.SecretKey = model.ApiKey;
        stripePaymentElementSettings.PublishableKey = model.PublishableKey;
        stripePaymentElementSettings.Theme = model.Theme;
        stripePaymentElementSettings.Layout = model.Layout;
        stripePaymentElementSettings.EnableLogging = model.EnableLogging;

        await _settingService.SaveSettingOverridablePerStoreAsync(stripePaymentElementSettings, x => x.TransactionMode, model.TransactionModeId_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(stripePaymentElementSettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(stripePaymentElementSettings, x => x.AdditionalFeePercentage, model.AdditionalFeePercentage_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(stripePaymentElementSettings, x => x.SecretKey, model.ApiKey_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(stripePaymentElementSettings, x => x.PublishableKey, model.PublishableKey_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(stripePaymentElementSettings, x => x.Theme, model.Theme_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(stripePaymentElementSettings, x => x.Layout, model.Layout_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(stripePaymentElementSettings, x => x.EnableLogging, model.EnableLogging_OverrideForStore, storeScope, false);

        //now clear settings cache
        await _settingService.ClearCacheAsync();

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));
        return await Configure();
    }

    [EditAccess]
    public async Task<IActionResult> UploadFile(IFormFile importexcelfile)
    {
        if (!await _permissionService.AuthorizeAsync(StripePaymentElementsPermissionProvider.ManageConfiguration))
            return AccessDeniedView();

        if (importexcelfile != null && importexcelfile.Length > 0)
        {
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var filePath = _nopFileProvider.MapPath(string.Format(StripeDefaults.AppleVerificationFilePath, storeScope));
            using Stream fileStream = new FileStream(filePath, FileMode.Create);
            await importexcelfile.CopyToAsync(fileStream);
        }
        else
        {
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.NopStation.StripePaymentElement.Configuration.UnsuccessfulUpload"));
            return RedirectToAction("Configure");
        }

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.NopStation.StripePaymentElement.Configuration.SuccessfulUpload"));

        return RedirectToAction("Configure");
    }

    #endregion
}