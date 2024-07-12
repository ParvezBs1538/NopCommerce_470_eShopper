using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;
using System;
using NopStation.Plugin.Widgets.Mouseflow.Components;

namespace NopStation.Plugin.Widgets.Mouseflow
{
    public class MouseflowPlugin : BasePlugin, IWidgetPlugin, INopStationPlugin, IAdminMenuPlugin
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;
        private readonly INopStationCoreService _nopStationCoreService;

        #endregion

        #region Ctor

        public MouseflowPlugin(ILocalizationService localizationService,
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
            return _webHelper.GetStoreLocation() + "Admin/Mouseflow/Configure";
        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            return typeof(MouseflowViewComponent);
        }

        public bool HideInWidgetList => false;
        
        public override async Task InstallAsync()
        {
            await this.InstallPluginAsync(new MouseflowPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            //settings
            await _settingService.DeleteSettingAsync<MouseflowSettings>();

            await this.UninstallPluginAsync(new MouseflowPermissionProvider());
            await base.UninstallAsync();
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            return new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Admin.NopStation.Mouseflow.Configuration.Instructions", "<p>Collect Mouseflow tracking code or website id from <a href=\"https://eu.mouseflow.com/websites/\">here</a>.</p>"),

                new KeyValuePair<string, string>("Admin.NopStation.Mouseflow.Configuration.Fields.EnablePlugin", "Enable plugin"),
                new KeyValuePair<string, string>("Admin.NopStation.Mouseflow.Configuration.Fields.EnablePlugin.Hint", "Determines whether the plugin is enabled or not."),
                new KeyValuePair<string, string>("Admin.NopStation.Mouseflow.Configuration.Fields.WebsiteId", "Website id"),
                new KeyValuePair<string, string>("Admin.NopStation.Mouseflow.Configuration.Fields.WebsiteId.Hint", "Enter Mouseflow website id."),
                new KeyValuePair<string, string>("Admin.NopStation.Mouseflow.Configuration.Fields.Script", "Script"),
                new KeyValuePair<string, string>("Admin.NopStation.Mouseflow.Configuration.Fields.Script.Hint", "This field is for script."),
                new KeyValuePair<string, string>("Admin.NopStation.Mouseflow.Configuration.Fields.SettingMode", "Setting mode"),
                new KeyValuePair<string, string>("Admin.NopStation.Mouseflow.Configuration.Fields.SettingMode.Hint", "Select setting mode."),

                new KeyValuePair<string, string>("Admin.NopStation.Mouseflow.Configuration", "Mouseflow settings"),
                new KeyValuePair<string, string>("Admin.NopStation.Mouseflow.Menu.Configuration", "Configuration"),
                new KeyValuePair<string, string>("Admin.NopStation.Mouseflow.Menu.Mouseflow", "Mouseflow"),
            };
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Mouseflow.Menu.Mouseflow"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(MouseflowPermissionProvider.ManageConfiguration))
            {
                var conItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Mouseflow.Menu.Configuration"),
                    Url = "~/Admin/Mouseflow/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "Mouseflow.Configuration"
                };
                menuItem.ChildNodes.Add(conItem);
            }
            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/mouseflow-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=mouseflow",
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
