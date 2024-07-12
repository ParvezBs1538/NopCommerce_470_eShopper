using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;

namespace NopStation.Plugin.Misc.AdminApi;

public class AdminApiPlugin : BasePlugin, IAdminMenuPlugin, INopStationPlugin
{
    #region Fields

    private readonly IWebHelper _webHelper;
    private readonly IPermissionService _permissionService;
    private readonly ILocalizationService _localizationService;
    private readonly INopStationCoreService _nopStationCoreService;

    #endregion

    #region Ctor

    public AdminApiPlugin(IWebHelper webHelper,
        IPermissionService permissionService,
        ILocalizationService localizationService,
        INopStationCoreService nopStationCoreService)
    {
        _webHelper = webHelper;
        _permissionService = permissionService;
        _localizationService = localizationService;
        _nopStationCoreService = nopStationCoreService;
    }

    #endregion

    #region Properties

    public static bool CheckGuestCustomer => false;

    #endregion

    #region Methods

    public override string GetConfigurationPageUrl()
    {
        return $"{_webHelper.GetStoreLocation()}Admin/AdminApi/Configure";
    }

    public override async Task InstallAsync()
    {
        await this.InstallPluginAsync(new AdminApiPermissionProvider());
        await base.InstallAsync();
    }

    public override async Task UninstallAsync()
    {
        await this.UninstallPluginAsync(new AdminApiPermissionProvider());
        await base.UninstallAsync();
    }

    public async Task ManageSiteMapAsync(SiteMapNode rootNode)
    {
        var menu = new SiteMapNode()
        {
            Visible = true,
            IconClass = "far fa-dot-circle",
            Title = await _localizationService.GetResourceAsync("Admin.NopStation.AdminApi.Menu.AdminApi")
        };

        if (await _permissionService.AuthorizeAsync(AdminApiPermissionProvider.ManageConfiguration))
        {
            var configure = new SiteMapNode()
            {
                Visible = true,
                IconClass = "far fa-circle",
                Url = _webHelper.GetStoreLocation() + "Admin/AdminApi/Configure",
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.AdminApi.Menu.Configuration"),
                SystemName = "AdminApi.Configuration"
            };
            menu.ChildNodes.Add(configure);
        }
        if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
        {
            var documentation = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                Url = "https://www.nop-station.com/admin-api-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=admin-api",
                Visible = true,
                IconClass = "far fa-circle",
                OpenUrlInNewTab = true
            };
            menu.ChildNodes.Add(documentation);
        }

        await _nopStationCoreService.ManageSiteMapAsync(rootNode, menu, NopStationMenuType.Plugin);
    }

    public List<KeyValuePair<string, string>> PluginResouces()
    {
        return [
            new ("Admin.Common.Loadingdialog.Title", "Please wait"),
            new ("Admin.NopStation.AdminApi.Menu.AdminApi", "Admin api"),
            new ("Admin.NopStation.AdminApi.Menu.Configuration", "Configuration"),

            new ("Admin.NopStation.AdminApi.Configuration.Title", "Web api settings"),
            new ("Admin.NopStation.AdminApi.Configuration.BlockTitle.Security", "Security"),
            new ("Admin.NopStation.AdminApi.Configuration.BlockTitle.AppSettings", "App settings"),

            new ("Admin.NopStation.AdminApi.Configuration.Fields.EnableCORS", "Enable CORS"),
            new ("Admin.NopStation.AdminApi.Configuration.Fields.EnableCORS.Hint", "Check to enable CORS. It will add \"Access-Control-Allow-Origin\" header for every api response."),
            new ("Admin.NopStation.AdminApi.Configuration.Fields.EnableJwtSecurity", "Enable JWT security"),
            new ("Admin.NopStation.AdminApi.Configuration.Fields.EnableJwtSecurity.Hint", "Check to enable JWT security. It will require 'NST' header for every api request."),
            new ("Admin.NopStation.AdminApi.Configuration.Fields.AndroidVersion", "Android version"),
            new ("Admin.NopStation.AdminApi.Configuration.Fields.AndroidVersion.Hint", "Current android version published in Google Play Store."),
            new ("Admin.NopStation.AdminApi.Configuration.Fields.AndriodForceUpdate", "Andriod force update"),
            new ("Admin.NopStation.AdminApi.Configuration.Fields.AndriodForceUpdate.Hint", "By marking it as checked, Android users will be forced to update their app when it will not match with current version published in Play Store."),
            new ("Admin.NopStation.AdminApi.Configuration.Fields.PlayStoreUrl", "Play Store url"),
            new ("Admin.NopStation.AdminApi.Configuration.Fields.PlayStoreUrl.Hint", "The Play Store url for your app."),
            new ("Admin.NopStation.AdminApi.Configuration.Fields.IOSVersion", "iOS Version"),
            new ("Admin.NopStation.AdminApi.Configuration.Fields.IOSVersion.Hint", "Current iOS version published in App Store."),
            new ("Admin.NopStation.AdminApi.Configuration.Fields.IOSForceUpdate", "iOS force update"),
            new ("Admin.NopStation.AdminApi.Configuration.Fields.IOSForceUpdate.Hint", "By marking it as checked, iOS users will be forced to update their app when it will not match with current version published in App Store."),
            new ("Admin.NopStation.AdminApi.Configuration.Fields.AppStoreUrl", "App Store url"),
            new ("Admin.NopStation.AdminApi.Configuration.Fields.AppStoreUrl.Hint", "The App Store url for your app."),
            new ("Admin.NopStation.AdminApi.Configuration.Fields.LogoId", "Logo"),
            new ("Admin.NopStation.AdminApi.Configuration.Fields.LogoId.Hint", "The logo which will be displayed in mobile app."),
            new ("Admin.NopStation.AdminApi.Configuration.Fields.LogoSize", "Logo Size"),
            new ("Admin.NopStation.AdminApi.Configuration.Fields.LogoSize.Hint", "Size of the logo you want to display"),
            new ("Admin.NopStation.AdminApi.Configuration.Fields.ShowChangeBaseUrlPanel", "Show change base url panel"),
            new ("Admin.NopStation.AdminApi.Configuration.Fields.ShowChangeBaseUrlPanel.Hint", "Determines whether admin user can change api base url from their app. Enable this option when you are in testing mode."),

            new ("Admin.NopStation.AdminApi.Configuration.Fields.SecretKey", "Secret key"),
            new ("Admin.NopStation.AdminApi.Configuration.Fields.SecretKey.Hint", "The secret key to sign and verify each JWT token, it can be any string."),
            new ("Admin.NopStation.AdminApi.Configuration.Fields.TokenKey", "Token key"),
            new ("Admin.NopStation.AdminApi.Configuration.Fields.TokenKey.Hint", "The JSON web token security key (payload: NST-KEY)."),
            new ("Admin.NopStation.AdminApi.Configuration.Fields.TokenSecret", "Token secret"),
            new ("Admin.NopStation.AdminApi.Configuration.Fields.TokenSecret.Hint", "512 bit JSON web token secret."),
            new ("Admin.NopStation.AdminApi.Configuration.Fields.CheckIat", "Check IAT"),
            new ("Admin.NopStation.AdminApi.Configuration.Fields.CheckIat.Hint", "Click to check issued at time."),
            new ("Admin.NopStation.AdminApi.Configuration.Fields.TokenSecondsValid", "JSON web token. Seconds valid"),
            new ("Admin.NopStation.AdminApi.Configuration.Fields.TokenSecondsValid.Hint", "Enter number of seconds for valid JSON web token."),

            new ("NopStation.AdminApi.Response.InvalidJwtToken", "Security token is expired or not valid"),
            new ("NopStation.AdminApi.Response.InvalidToken", "Token has been expired. Please login again"),
            new ("NopStation.AdminApi.Response.InvalidLicense", "Invalid nop-station product license"),
            new ("NopStation.AdminApi.Response.Unauthorized", "Unauthorized to access the resources"),
        ];
    }

    #endregion
}
