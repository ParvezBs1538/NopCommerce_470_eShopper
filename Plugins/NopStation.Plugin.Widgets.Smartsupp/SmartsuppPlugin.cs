using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Widgets.Smartsupp.Components;

namespace NopStation.Plugin.Widgets.Smartsupp
{
    public class SmartsuppPlugin : BasePlugin, IWidgetPlugin, INopStationPlugin, IAdminMenuPlugin
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;
        private readonly INopStationCoreService _nopStationCoreService;

        #endregion

        #region Ctor

        public SmartsuppPlugin(ILocalizationService localizationService,
            IWebHelper webHelper,
            ISettingService settingService,
            IPermissionService permissionService,
            INopStationCoreService nopStationCoreService)
        {
            _localizationService = localizationService;
            _webHelper = webHelper;
            _settingService = settingService;
            _permissionService = permissionService;
            _nopStationCoreService = nopStationCoreService;
        }

        #endregion

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string>
            {
                PublicWidgetZones.Footer
            });
        }

        public override string GetConfigurationPageUrl()
        {
            return _webHelper.GetStoreLocation() + "Admin/Smartsupp/Configure";
        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            return typeof(SmartsuppViewComponent);
        }

        public bool HideInWidgetList => false;

        public override async Task InstallAsync()
        {
            //settings
            await _settingService.SaveSettingAsync(new SmartsuppSettings
            {
                SettingMode = SettingMode.Key
            });

            await this.InstallPluginAsync(new SmartsuppPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            //settings
            await _settingService.DeleteSettingAsync<SmartsuppSettings>();

            await this.UninstallPluginAsync(new SmartsuppPermissionProvider());
            await base.UninstallAsync();
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            return new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Admin.NopStation.Smartsupp.Configuration.Instructions", "<p>Collect Smartsupp chat script or key from <a href=\"https://app.smartsupp.com/app/settings/chatbox/chat-code\">here</a>.</p>"),

                new KeyValuePair<string, string>("Admin.NopStation.Smartsupp.Configuration.Fields.EnablePlugin", "Enable plugin"),
                new KeyValuePair<string, string>("Admin.NopStation.Smartsupp.Configuration.Fields.EnablePlugin.Hint", "Determines whether the plugin is enabled or not."),
                new KeyValuePair<string, string>("Admin.NopStation.Smartsupp.Configuration.Fields.Key", "Key"),
                new KeyValuePair<string, string>("Admin.NopStation.Smartsupp.Configuration.Fields.Key.Hint", "Enter Smartsupp key."),
                new KeyValuePair<string, string>("Admin.NopStation.Smartsupp.Configuration.Fields.Script", "Script"),
                new KeyValuePair<string, string>("Admin.NopStation.Smartsupp.Configuration.Fields.Script.Hint", "This field for script."),
                new KeyValuePair<string, string>("Admin.NopStation.Smartsupp.Configuration.Fields.SettingMode", "Setting mode"),
                new KeyValuePair<string, string>("Admin.NopStation.Smartsupp.Configuration.Fields.SettingMode.Hint", "Select setting mode."),

                new KeyValuePair<string, string>("Admin.NopStation.Smartsupp.Configuration", "Smartsupp settings"),
                new KeyValuePair<string, string>("Admin.NopStation.Smartsupp.Menu.Configuration", "Configuration"),
                new KeyValuePair<string, string>("Admin.NopStation.Smartsupp.Menu.Smartsupp", "Smartsupp"),
            };
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Smartsupp.Menu.Smartsupp"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(SmartsuppPermissionProvider.ManageConfiguration))
            {
                var conItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Smartsupp.Menu.Configuration"),
                    Url = "~/Admin/Smartsupp/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "Smartsupp.Configuration"
                };
                menuItem.ChildNodes.Add(conItem);
            }

            var documentation = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                Url = "https://www.nop-station.com/smartsupp-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=smartsupp",
                Visible = true,
                IconClass = "far fa-circle",
                OpenUrlInNewTab = true
            };
            menuItem.ChildNodes.Add(documentation);

            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menuItem, NopStationMenuType.Plugin);
        }
    }
}
