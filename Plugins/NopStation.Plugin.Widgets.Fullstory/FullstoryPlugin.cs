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
using NopStation.Plugin.Widgets.Fullstory.Components;

namespace NopStation.Plugin.Widgets.Fullstory
{
    public class FullstoryPlugin : BasePlugin, IWidgetPlugin, INopStationPlugin, IAdminMenuPlugin
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;
        private readonly INopStationCoreService _nopStationCoreService;

        #endregion

        #region Ctor

        public FullstoryPlugin(ILocalizationService localizationService,
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
            return _webHelper.GetStoreLocation() + "Admin/Fullstory/Configure";
        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            return typeof(FullstoryViewComponent);
        }

        public bool HideInWidgetList => false;
        
        public override async Task InstallAsync()
        {
            await this.InstallPluginAsync(new FullstoryPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            //settings
            await _settingService.DeleteSettingAsync<FullstorySettings>();

            await this.UninstallPluginAsync(new FullstoryPermissionProvider());
            await base.UninstallAsync();
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            return new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Admin.NopStation.Fullstory.Configuration.Instructions", "<p>Collect Fullstory tracking code or organization id from <a href=\"https://app.fullstory.com/ui/\">here</a>.</p>"),

                new KeyValuePair<string, string>("Admin.NopStation.Fullstory.Configuration.Fields.EnablePlugin", "Enable plugin"),
                new KeyValuePair<string, string>("Admin.NopStation.Fullstory.Configuration.Fields.EnablePlugin.Hint", "Determines whether the plugin is enabled or not."),
                new KeyValuePair<string, string>("Admin.NopStation.Fullstory.Configuration.Fields.OrganizationId", "Organization id"),
                new KeyValuePair<string, string>("Admin.NopStation.Fullstory.Configuration.Fields.OrganizationId.Hint", "Enter Fullstory organization id."),
                new KeyValuePair<string, string>("Admin.NopStation.Fullstory.Configuration.Fields.Script", "Script"),
                new KeyValuePair<string, string>("Admin.NopStation.Fullstory.Configuration.Fields.Script.Hint", "This field is for script."),
                new KeyValuePair<string, string>("Admin.NopStation.Fullstory.Configuration.Fields.SettingMode", "Setting mode"),
                new KeyValuePair<string, string>("Admin.NopStation.Fullstory.Configuration.Fields.SettingMode.Hint", "Select setting mode."),

                new KeyValuePair<string, string>("Admin.NopStation.Fullstory.Configuration", "Fullstory settings"),
                new KeyValuePair<string, string>("Admin.NopStation.Fullstory.Menu.Configuration", "Configuration"),
                new KeyValuePair<string, string>("Admin.NopStation.Fullstory.Menu.Fullstory", "Fullstory"),
            };
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Fullstory.Menu.Fullstory"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(FullstoryPermissionProvider.ManageConfiguration))
            {
                var conItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Fullstory.Menu.Configuration"),
                    Url = "~/Admin/Fullstory/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "Fullstory.Configuration"
                };
                menuItem.ChildNodes.Add(conItem);
            }
            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/fullstory-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=fullstory",
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
