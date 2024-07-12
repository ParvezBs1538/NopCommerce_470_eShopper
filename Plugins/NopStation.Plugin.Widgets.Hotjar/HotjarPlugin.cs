using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Helpers;
using NopStation.Plugin.Misc.Core.Services;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Widgets.Hotjar.Components;
using System;

namespace NopStation.Plugin.Widgets.Hotjar
{
    public class HotjarPlugin : BasePlugin, IWidgetPlugin, INopStationPlugin, IAdminMenuPlugin
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;
        private readonly INopStationCoreService _nopStationCoreService;

        #endregion

        #region Ctor

        public HotjarPlugin(ILocalizationService localizationService,
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
            return _webHelper.GetStoreLocation() + "Admin/Hotjar/Configure";
        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            return typeof(HotjarViewComponent);
        }

        public bool HideInWidgetList => false;
        
        public override async Task InstallAsync()
        {
            await this.InstallPluginAsync(new HotjarPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            //settings
            await _settingService.DeleteSettingAsync<HotjarSettings>();

            await this.UninstallPluginAsync(new HotjarPermissionProvider());
            await base.UninstallAsync();
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            return new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Admin.NopStation.Hotjar.Configuration.Instructions", "<p>Collect Hotjar script or site id from <a href=\"https://www.hotjar.com/\">here</a>.</p>"),

                new KeyValuePair<string, string>("Admin.NopStation.Hotjar.Configuration.Fields.EnablePlugin", "Enable plugin"),
                new KeyValuePair<string, string>("Admin.NopStation.Hotjar.Configuration.Fields.EnablePlugin.Hint", "Determines whether the plugin is enabled or not."),
                new KeyValuePair<string, string>("Admin.NopStation.Hotjar.Configuration.Fields.SiteId", "Site id"),
                new KeyValuePair<string, string>("Admin.NopStation.Hotjar.Configuration.Fields.SiteId.Hint", "Enter Hotjar site id."),
                new KeyValuePair<string, string>("Admin.NopStation.Hotjar.Configuration.Fields.Script", "Script"),
                new KeyValuePair<string, string>("Admin.NopStation.Hotjar.Configuration.Fields.Script.Hint", "This field is for script."),
                new KeyValuePair<string, string>("Admin.NopStation.Hotjar.Configuration.Fields.SettingMode", "Setting mode"),
                new KeyValuePair<string, string>("Admin.NopStation.Hotjar.Configuration.Fields.SettingMode.Hint", "Select setting mode."),

                new KeyValuePair<string, string>("Admin.NopStation.Hotjar.Configuration", "Hotjar settings"),
                new KeyValuePair<string, string>("Admin.NopStation.Hotjar.Menu.Configuration", "Configuration"),
                new KeyValuePair<string, string>("Admin.NopStation.Hotjar.Menu.Hotjar", "Hotjar"),
            };
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Hotjar.Menu.Hotjar"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(HotjarPermissionProvider.ManageConfiguration))
            {
                var conItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Hotjar.Menu.Configuration"),
                    Url = "~/Admin/Hotjar/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "Hotjar.Configuration"
                };
                menuItem.ChildNodes.Add(conItem);
            }
            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/hotjar-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=hotjar",
                    Visible = true,
                    IconClass = "far fa-circle",
                    OpenUrlInNewTab = true
                };
                menuItem.ChildNodes.Add(documentation);
            } 

            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menuItem, NopStationMenuType.Plugin);
        }
    }
}
