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
using NopStation.Plugin.Widgets.Clarity.Components;

namespace NopStation.Plugin.Widgets.Clarity
{
    public class ClarityPlugin : BasePlugin, IWidgetPlugin, INopStationPlugin, IAdminMenuPlugin
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;
        private readonly INopStationCoreService _nopStationCoreService;

        #endregion

        #region Ctor

        public ClarityPlugin(ILocalizationService localizationService,
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
                PublicWidgetZones.HeadHtmlTag
            });
        }

        public override string GetConfigurationPageUrl()
        {
            return _webHelper.GetStoreLocation() + "Admin/Clarity/Configure";
        }

        public bool HideInWidgetList => false;

        public override async Task InstallAsync()
        {
            await this.InstallPluginAsync(new ClarityPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            //settings
            await _settingService.DeleteSettingAsync<ClaritySettings>();

            await this.UninstallPluginAsync(new ClarityPermissionProvider());
            await base.UninstallAsync();
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            return new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Admin.NopStation.Clarity.Configuration.Instructions", "<p>Collect Clarity tracking code or project id from <a href=\"https://clarity.microsoft.com/projects/\">here</a>.</p>"),

                new KeyValuePair<string, string>("Admin.NopStation.Clarity.Configuration.Fields.EnablePlugin", "Enable plugin"),
                new KeyValuePair<string, string>("Admin.NopStation.Clarity.Configuration.Fields.EnablePlugin.Hint", "Determines whether the plugin is enabled or not."),
                new KeyValuePair<string, string>("Admin.NopStation.Clarity.Configuration.Fields.ProjectId", "Project id"),
                new KeyValuePair<string, string>("Admin.NopStation.Clarity.Configuration.Fields.ProjectId.Hint", "Enter Clarity project id."),
                new KeyValuePair<string, string>("Admin.NopStation.Clarity.Configuration.Fields.TrackingCode", "Tracking code"),
                new KeyValuePair<string, string>("Admin.NopStation.Clarity.Configuration.Fields.TrackingCode.Hint", "This field is for tracking code."),
                new KeyValuePair<string, string>("Admin.NopStation.Clarity.Configuration.Fields.SettingMode", "Setting mode"),
                new KeyValuePair<string, string>("Admin.NopStation.Clarity.Configuration.Fields.SettingMode.Hint", "Select setting mode."),

                new KeyValuePair<string, string>("Admin.NopStation.Clarity.Configuration", "Clarity settings"),
                new KeyValuePair<string, string>("Admin.NopStation.Clarity.Menu.Configuration", "Configuration"),
                new KeyValuePair<string, string>("Admin.NopStation.Clarity.Menu.Clarity", "Clarity"),
            };
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Clarity.Menu.Clarity"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(ClarityPermissionProvider.ManageConfiguration))
            {
                var conItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Clarity.Menu.Configuration"),
                    Url = "~/Admin/Clarity/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "Clarity.Configuration"
                };
                menuItem.ChildNodes.Add(conItem);
            }
            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/microsoft-clarity-integration-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=microsoft-clarity-integration",
                    Visible = true,
                    IconClass = "far fa-circle",
                    OpenUrlInNewTab = true
                };
                menuItem.ChildNodes.Add(documentation);
            }

            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menuItem, NopStationMenuType.Plugin);
        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            return typeof(ClarityViewComponent);
        }
    }
}
