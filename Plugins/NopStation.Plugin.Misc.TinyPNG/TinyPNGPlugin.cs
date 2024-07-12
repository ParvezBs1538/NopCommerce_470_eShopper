using Nop.Core;
using NopStation.Plugin.Misc.Core.Services;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework.Menu;
using System.Threading.Tasks;
using NopStation.Plugin.Misc.Core;
using System.Collections.Generic;
using Nop.Services.Security;
using NopStation.Plugin.Misc.TinyPNG.Extensions;

namespace NopStation.Plugin.Misc.TinyPNG
{
    public class TinyPNGPlugin : BasePlugin, IMiscPlugin, IAdminMenuPlugin, INopStationPlugin
    {
        #region Fields
        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly ISettingService _settingService;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly IPermissionService _permissionService;
        #endregion

        #region Properties
        public bool HideInWidgetList => false;
        #endregion

        #region Ctor
        public TinyPNGPlugin(ILocalizationService localizationService,
            IWebHelper webHelper,
            ISettingService settingService,
            INopStationCoreService nopStationCoreService,
            IPermissionService permissionService
            )
        {
            _localizationService = localizationService;
            _webHelper = webHelper;
            _settingService = settingService;
            _nopStationCoreService = nopStationCoreService;
            _permissionService = permissionService;
        }
        #endregion

        #region Methods

        public override string GetConfigurationPageUrl()
        {
            return _webHelper.GetStoreLocation() + "Admin/TinyPNG/Configure";
        }

        public override async Task InstallAsync()
        {
            //settings
            var settings = new TinyPNGSettings
            {
                TinyPNGEnable = true,
                ApiUrl = "https://api.tinify.com/shrink"
            };
            await _settingService.SaveSettingAsync(settings);

            await this.InstallPluginAsync(new TinyPNGPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await this.UninstallPluginAsync(new TinyPNGPermissionProvider());
            await base.UninstallAsync();
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menu = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.TinyPNG.Menu.TinyPNG"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(TinyPNGPermissionProvider.ManageConfiguration))
            {
                var configurationItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.TinyPNG.Menu.Configuration"),
                    Url = "~/Admin/TinyPNG/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "TinyPNG.Configuration"
                };
                menu.ChildNodes.Add(configurationItem);
            }

            var documentation = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                Url = "https://www.nop-station.com/tiny-png-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=tiny-png",
                Visible = true,
                IconClass = "far fa-circle",
                OpenUrlInNewTab = true
            };
            menu.ChildNodes.Add(documentation);

            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menu, NopStationMenuType.Plugin);
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Admin.NopStation.TinyPNG.Menu.TinyPNG", "Tiny PNG"),
                new KeyValuePair<string, string>("Admin.NopStation.TinyPNG.Menu.Configuration", "Configuration"),

                new KeyValuePair<string, string>("Admin.NopStation.TinyPNG.Configuration", "Tiny PNG settings"),
                new KeyValuePair<string, string>("Admin.NopStation.TinyPNG.Configuration.Fields.Enable", "Enable"),
                new KeyValuePair<string, string>("Admin.NopStation.TinyPNG.Configuration.Fields.Enable.Hint", "Enable or disable this plugin funtion"),
                new KeyValuePair<string, string>("Admin.NopStation.TinyPNG.Configuration.Fields.ApiUrl", "Api url"),
                new KeyValuePair<string, string>("Admin.NopStation.TinyPNG.Configuration.Fields.ApiUrl.Hint", "Tiny PNG api url"),
                new KeyValuePair<string, string>("Admin.NopStation.TinyPNG.Configuration.Fields.Keys", "Keys"),
                new KeyValuePair<string, string>("Admin.NopStation.TinyPNG.Configuration.Fields.Keys.Hint", "Add multiple Tiny PNG keys by comma seprated text"),

                new KeyValuePair<string, string>("Admin.NopStation.TinyPNG.Configuration.Instructions",
                "<ul><li>Register for Developer API key by sign up to <a href=\"https://tinypng.com/developers\" target=\"_blank\">Developer API</a></li><li>Verify your email by clicking button \"Visit your Dashboard\".</li><li>Copy API key from your API Dashboard</li></ul>"),
            };

            return list;
        }

        #endregion
    }
}
