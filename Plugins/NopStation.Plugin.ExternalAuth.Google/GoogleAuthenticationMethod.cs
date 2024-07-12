using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Services.Authentication.External;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.ExternalAuth.Google.Components;
using NopStation.Plugin.Misc.AlgoliaSearch.Extensions;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;

namespace NopStation.Plugin.ExternalAuth.Google
{
    public class GoogleAuthenticationMethod : BasePlugin, IExternalAuthenticationMethod, INopStationPlugin, IAdminMenuPlugin
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly IPermissionService _permissionService;
        private readonly INopStationCoreService _nopStationCoreService;

        #endregion

        #region Ctor

        public GoogleAuthenticationMethod(ILocalizationService localizationService,
            ISettingService settingService,
            IWebHelper webHelper,
            INopStationCoreService nopStationCoreService, IPermissionService permissionService)
        {
            _localizationService = localizationService;
            _settingService = settingService;
            _webHelper = webHelper;
            _nopStationCoreService = nopStationCoreService;
            _permissionService = permissionService;
        }

        #endregion

        #region Methods

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/GoogleAuthConfig/Configure";
        }

        public Type GetPublicViewComponent()
        {
            return typeof(GoogleAuthenticationViewComponent);
        }

        public override async Task InstallAsync()
        {
            //settings
            await _settingService.SaveSettingAsync(new GoogleExternalAuthSettings());
            await this.InstallPluginAsync(new GoogleAuthenticationPermissionProvider());
            await base.InstallAsync();
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menu = new SiteMapNode()
            {
                Visible = true,
                IconClass = "far fa-dot-circle",
                Title = await _localizationService.GetResourceAsync("NopStation.Plugin.ExternalAuth.Google.Menu")
            };

            if (await _permissionService.AuthorizeAsync(GoogleAuthenticationPermissionProvider.ManageConfiguration))
            {
                var configuration = new SiteMapNode()
                {
                    Visible = true,
                    IconClass = "far fa-circle",
                    Url = "~/Admin/GoogleAuthConfig/Configure",
                    Title = await _localizationService.GetResourceAsync("NopStation.Plugin.ExternalAuth.Google.Menu.Configuration"),
                    SystemName = "GoogleAuthentication.Configuration"
                };
                menu.ChildNodes.Add(configuration);
            }
            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/google-externalauth-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=google-externalauth",
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
            return new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("NopStation.Plugin.ExternalAuth.Google.AuthenticationDataDeletedSuccessfully", "Data deletion request completed"),
                new KeyValuePair<string, string>("NopStation.Plugin.ExternalAuth.Google.AuthenticationDataExist", "Data deletion request is pending, please contact the admin"),
                new KeyValuePair<string, string>("NopStation.Plugin.ExternalAuth.Google.ClientKeyIdentifier", "Client ID/API Key"),
                new KeyValuePair<string, string>("NopStation.Plugin.ExternalAuth.Google.ClientKeyIdentifier.Hint", "Enter your Client ID/API key here. You can find it on your Google application page."),
                new KeyValuePair<string, string>("NopStation.Plugin.ExternalAuth.Google.ClientSecret", "Client Secret"),
                new KeyValuePair<string, string>("NopStation.Plugin.ExternalAuth.Google.ClientSecret.Hint", "Enter your Clint secret here. You can find it on your Google application page."),
                new KeyValuePair<string, string>("NopStation.Plugin.ExternalAuth.Google.Menu", "Google Authentication"),
                new KeyValuePair<string, string>("NopStation.Plugin.ExternalAuth.Google.Menu.Configuration", "Configure"),
                new KeyValuePair<string, string>("NopStation.Plugin.ExternalAuth.Google.Configuration", "Google authentication settings"),
                new KeyValuePair<string, string>("NopStation.Plugin.ExternalAuth.Google.Instructions",
                    "<p>To configure authentication with Google, please follow these steps:<br/>" +
                    "<ol><li>Open the <a href=\"https://console.cloud.google.com/\" target=\"_blank\">Google Cloud Platform Console</a> and create a new project." +
                    "<ul><li>Click on the sidebar in the top left corner.</li><li>Navigate to APIs & Services > Credentials.</li></ul></li>" +
                    "<li>Create new credentials:" +
                    "<ul><li>Click on <b>+</b> Create Credentials.</li><li>Select <b>OAuth client ID</b> from the list of options.</li></ul></li>" +
                    "<li>Configure your application type:" +
                    "<ul><li>Choose the application type <b>Web application</b>.</li></ul></li>" +
                    "<li>Enter a descriptive name for your OAuth ID in the Name field. This name is only visible in the Cloud Console." +
                    "<br/>In <b>Authorized redirect URIs</b> input field put <b>www.yourwebsiteurl.com/signin-google</b>.<br/>Replace <b>www.yourwebsiteurl.com</b> with your website URL.</li>" +
                    "<li>Click Create. Your OAuth ID and client secret will be displayed. Take note of both, as the client secret needs to be kept <b>confidential</b>." +
                    " Click Download JSON to save a local copy of your credentials.</li></ul><br/><br/></p>"
                )
            };
        }

        public override async Task UninstallAsync()
        {
            //settings
            await _settingService.DeleteSettingAsync<GoogleExternalAuthSettings>();

            //locales
            await _localizationService.DeleteLocaleResourcesAsync("Plugins.ExternalAuth.Google");

            await base.UninstallAsync();
        }

        #endregion
    }
}