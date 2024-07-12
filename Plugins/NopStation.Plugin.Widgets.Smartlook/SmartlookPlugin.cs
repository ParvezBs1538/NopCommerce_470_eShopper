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
using NopStation.Plugin.Widgets.Smartlook.Components;

namespace NopStation.Plugin.Widgets.Smartlook
{
    public class SmartlookPlugin : BasePlugin, IWidgetPlugin, INopStationPlugin, IAdminMenuPlugin
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;
        private readonly INopStationCoreService _nopStationCoreService;

        #endregion

        #region Ctor

        public SmartlookPlugin(ILocalizationService localizationService,
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
            return _webHelper.GetStoreLocation() + "Admin/Smartlook/Configure";
        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            return typeof(SmartlookViewComponent);
        }

        public bool HideInWidgetList => false;

        public override async Task InstallAsync()
        {
            await this.InstallPluginAsync(new SmartlookPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            //settings
            await _settingService.DeleteSettingAsync<SmartlookSettings>();

            await this.UninstallPluginAsync(new SmartlookPermissionProvider());
            await base.UninstallAsync();
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            return new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Admin.NopStation.Smartlook.Configuration.Instructions", "<p>Collect Smartlook script or site id from <a href=\"https://app.smartlook.com/\">here</a>.</p>"),

                new KeyValuePair<string, string>("Admin.NopStation.Smartlook.Configuration.Fields.EnablePlugin", "Enable plugin"),
                new KeyValuePair<string, string>("Admin.NopStation.Smartlook.Configuration.Fields.EnablePlugin.Hint", "Determines whether the plugin is enabled or not."),
                new KeyValuePair<string, string>("Admin.NopStation.Smartlook.Configuration.Fields.ProjectKey", "Project key"),
                new KeyValuePair<string, string>("Admin.NopStation.Smartlook.Configuration.Fields.ProjectKey.Hint", "Enter Smartlook project key."),
                new KeyValuePair<string, string>("Admin.NopStation.Smartlook.Configuration.Fields.Script", "Script"),
                new KeyValuePair<string, string>("Admin.NopStation.Smartlook.Configuration.Fields.Script.Hint", "This field is for script."),
                new KeyValuePair<string, string>("Admin.NopStation.Smartlook.Configuration.Fields.SettingMode", "Setting mode"),
                new KeyValuePair<string, string>("Admin.NopStation.Smartlook.Configuration.Fields.SettingMode.Hint", "Select setting mode."),

                new KeyValuePair<string, string>("Admin.NopStation.Smartlook.Configuration", "Smartlook settings"),
                new KeyValuePair<string, string>("Admin.NopStation.Smartlook.Menu.Configuration", "Configuration"),
                new KeyValuePair<string, string>("Admin.NopStation.Smartlook.Menu.Smartlook", "Smartlook"),
            };
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Smartlook.Menu.Smartlook"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(SmartlookPermissionProvider.ManageConfiguration))
            {
                var conItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Smartlook.Menu.Configuration"),
                    Url = "~/Admin/Smartlook/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "Smartlook.Configuration"
                };
                menuItem.ChildNodes.Add(conItem);
            }

            var documentation = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                Url = "https://www.nop-station.com/smartlook-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=smartlook",
                Visible = true,
                IconClass = "far fa-circle",
                OpenUrlInNewTab = true
            };
            menuItem.ChildNodes.Add(documentation);

            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menuItem, NopStationMenuType.Plugin);
        }
    }
}
