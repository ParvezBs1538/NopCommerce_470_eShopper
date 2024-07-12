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
using NopStation.Plugin.Widgets.Howuku.Components;

namespace NopStation.Plugin.Widgets.Howuku
{
    public class HowukuPlugin : BasePlugin, IWidgetPlugin, INopStationPlugin, IAdminMenuPlugin
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;
        private readonly INopStationCoreService _nopStationCoreService;

        #endregion

        #region Ctor

        public HowukuPlugin(ILocalizationService localizationService,
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
            return _webHelper.GetStoreLocation() + "Admin/Howuku/Configure";
        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            return typeof(HowukuViewComponent);
        }

        public bool HideInWidgetList => false;

        public override async Task InstallAsync()
        {
            await this.InstallPluginAsync(new HowukuPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            //settings
            await _settingService.DeleteSettingAsync<HowukuSettings>();

            await this.UninstallPluginAsync(new HowukuPermissionProvider());
            await base.UninstallAsync();
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            return new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Admin.NopStation.Howuku.Configuration.Instructions", "<p>Collect Howuku script or project key from <a href=\"https://app.howuku.com/overview\">here</a>.</p>"),

                new KeyValuePair<string, string>("Admin.NopStation.Howuku.Configuration.Fields.EnablePlugin", "Enable plugin"),
                new KeyValuePair<string, string>("Admin.NopStation.Howuku.Configuration.Fields.EnablePlugin.Hint", "Determines whether the plugin is enabled or not."),
                new KeyValuePair<string, string>("Admin.NopStation.Howuku.Configuration.Fields.ProjectKey", "App id"),
                new KeyValuePair<string, string>("Admin.NopStation.Howuku.Configuration.Fields.ProjectKey.Hint", "Enter Howuku app id."),
                new KeyValuePair<string, string>("Admin.NopStation.Howuku.Configuration.Fields.Script", "Script"),
                new KeyValuePair<string, string>("Admin.NopStation.Howuku.Configuration.Fields.Script.Hint", "This field is for script."),
                new KeyValuePair<string, string>("Admin.NopStation.Howuku.Configuration.Fields.SettingMode", "Setting mode"),
                new KeyValuePair<string, string>("Admin.NopStation.Howuku.Configuration.Fields.SettingMode.Hint", "Select setting mode."),

                new KeyValuePair<string, string>("Admin.NopStation.Howuku.Configuration", "Howuku settings"),
                new KeyValuePair<string, string>("Admin.NopStation.Howuku.Menu.Configuration", "Configuration"),
                new KeyValuePair<string, string>("Admin.NopStation.Howuku.Menu.Howuku", "Howuku"),
            };
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Howuku.Menu.Howuku"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(HowukuPermissionProvider.ManageConfiguration))
            {
                var conItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Howuku.Menu.Configuration"),
                    Url = "~/Admin/Howuku/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "Howuku.Configuration"
                };
                menuItem.ChildNodes.Add(conItem);
            }
            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/howuku-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=howuku",
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