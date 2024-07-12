using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using NopStation.Plugin.Misc.AdminApi.Areas.Admin.Factories;
using NopStation.Plugin.Misc.AdminApi.Areas.Admin.Models;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;

namespace NopStation.Plugin.Misc.AdminApi.Areas.Admin.Controllers;

public class AdminApiController : NopStationAdminController
{
    #region Fields

    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;
    private readonly ISettingService _settingService;
    private readonly IStoreContext _storeContext;
    private readonly IAdminApiModelFactory _webApiModelFactory;
    private readonly IPermissionService _permissionService;

    #endregion

    #region Ctor

    public AdminApiController(ILocalizationService localizationService,
        INotificationService notificationService,
        ISettingService settingService,
        IStoreContext storeContext,
        IAdminApiModelFactory webApiModelFactory,
        IPermissionService permissionService)
    {
        _localizationService = localizationService;
        _notificationService = notificationService;
        _settingService = settingService;
        _storeContext = storeContext;
        _webApiModelFactory = webApiModelFactory;
        _permissionService = permissionService;
    }

    #endregion

    #region Methods

    public async Task<IActionResult> Configure()
    {
        if (!await _permissionService.AuthorizeAsync(AdminApiPermissionProvider.ManageConfiguration))
            return AccessDeniedView();

        var model = await _webApiModelFactory.PrepareConfigurationModelAsync();
        return View("/Plugins/NopStation.Plugin.Misc.AdminApi/Areas/Admin/Views/AdminApi/Configure.cshtml", model);
    }

    [EditAccess, HttpPost]
    public async Task<IActionResult> Configure(ConfigurationModel model)
    {
        if (!await _permissionService.AuthorizeAsync(AdminApiPermissionProvider.ManageConfiguration))
            return AccessDeniedView();

        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var webApiSettings = await _settingService.LoadSettingAsync<AdminApiSettings>(storeScope);
        webApiSettings = model.ToSettings(webApiSettings);

        await _settingService.SaveSettingOverridablePerStoreAsync(webApiSettings, x => x.CheckIat, model.CheckIat_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(webApiSettings, x => x.EnableJwtSecurity, model.EnableJwtSecurity_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(webApiSettings, x => x.TokenKey, model.TokenKey_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(webApiSettings, x => x.SecretKey, model.SecretKey_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(webApiSettings, x => x.TokenSecondsValid, model.TokenSecondsValid_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(webApiSettings, x => x.TokenSecret, model.TokenSecret_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(webApiSettings, x => x.AndroidVersion, model.AndroidVersion_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(webApiSettings, x => x.AndriodForceUpdate, model.AndriodForceUpdate_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(webApiSettings, x => x.PlayStoreUrl, model.PlayStoreUrl_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(webApiSettings, x => x.IOSVersion, model.IOSVersion_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(webApiSettings, x => x.IOSForceUpdate, model.IOSForceUpdate_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(webApiSettings, x => x.AppStoreUrl, model.AppStoreUrl_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(webApiSettings, x => x.LogoId, model.LogoId_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(webApiSettings, x => x.LogoSize, model.LogoSize_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(webApiSettings, x => x.ShowChangeBaseUrlPanel, model.ShowChangeBaseUrlPanel_OverrideForStore, storeScope, false);

        await _settingService.ClearCacheAsync();

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

        return RedirectToAction("Configure");
    }

    #endregion
}
