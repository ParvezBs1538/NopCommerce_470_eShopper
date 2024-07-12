using System.Threading.Tasks;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using NopStation.Plugin.Misc.AdminApi.Areas.Admin.Models;

namespace NopStation.Plugin.Misc.AdminApi.Areas.Admin.Factories;

public class AdminApiModelFactory : IAdminApiModelFactory
{
    #region Fields

    private readonly ISettingService _settingService;
    private readonly IStoreContext _storeContext;

    #endregion

    #region Ctor

    public AdminApiModelFactory(ISettingService settingService,
        IStoreContext storeContext)
    {
        _settingService = settingService;
        _storeContext = storeContext;
    }

    #endregion

    #region Methods

    public async Task<ConfigurationModel> PrepareConfigurationModelAsync()
    {
        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var webApiSettings = await _settingService.LoadSettingAsync<AdminApiSettings>(storeScope);

        var model = webApiSettings.ToSettingsModel<ConfigurationModel>();
        model.ActiveStoreScopeConfiguration = storeScope;

        if (storeScope == 0)
            return model;

        model.CheckIat_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.CheckIat, storeScope);
        model.EnableJwtSecurity_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.EnableJwtSecurity, storeScope);
        model.SecretKey_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.SecretKey, storeScope);
        model.TokenKey_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.TokenKey, storeScope);
        model.TokenSecondsValid_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.TokenSecondsValid, storeScope);
        model.TokenSecret_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.TokenSecret, storeScope);
        model.AndroidVersion_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.AndroidVersion, storeScope);
        model.AndriodForceUpdate_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.AndriodForceUpdate, storeScope);
        model.PlayStoreUrl_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.PlayStoreUrl, storeScope);
        model.IOSVersion_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.IOSVersion, storeScope);
        model.IOSForceUpdate_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.IOSForceUpdate, storeScope);
        model.AppStoreUrl_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.AppStoreUrl, storeScope);
        model.LogoId_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.LogoId, storeScope);
        model.LogoSize_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.LogoSize, storeScope);
        model.ShowChangeBaseUrlPanel_OverrideForStore = await _settingService.SettingExistsAsync(webApiSettings, x => x.ShowChangeBaseUrlPanel, storeScope);

        return model;
    }

    #endregion
}
