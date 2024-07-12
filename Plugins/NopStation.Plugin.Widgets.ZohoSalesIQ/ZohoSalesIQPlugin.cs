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
using NopStation.Plugin.Widgets.ZohoSalesIQ.Components;

namespace NopStation.Plugin.Widgets.ZohoSalesIQ
{
    public class ZohoSalesIQPlugin : BasePlugin, IWidgetPlugin, INopStationPlugin, IAdminMenuPlugin
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;
        private readonly INopStationCoreService _nopStationCoreService;

        #endregion

        #region Ctor

        public ZohoSalesIQPlugin(ILocalizationService localizationService,
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
            return _webHelper.GetStoreLocation() + "Admin/ZohoSalesIQ/Configure";
        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            return typeof(ZohoSalesIQViewComponent);
        }

        public bool HideInWidgetList => false;

        public override async Task InstallAsync()
        {
            await this.InstallPluginAsync(new ZohoSalesIQPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            //settings
            await _settingService.DeleteSettingAsync<ZohoSalesIQSettings>();

            await this.UninstallPluginAsync(new ZohoSalesIQPermissionProvider());
            await base.UninstallAsync();
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            return new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Admin.NopStation.ZohoSalesIQ.Configuration.Instructions", "<p>Collect Zoho SalesIQ script from <a href=\"https://salesiq.zoho.com/\">here</a>.</p>"),

                new KeyValuePair<string, string>("Admin.NopStation.ZohoSalesIQ.Configuration.Fields.EnablePlugin", "Enable plugin"),
                new KeyValuePair<string, string>("Admin.NopStation.ZohoSalesIQ.Configuration.Fields.EnablePlugin.Hint", "Determines whether the plugin is enabled or not."),
                new KeyValuePair<string, string>("Admin.NopStation.ZohoSalesIQ.Configuration.Fields.Script", "Script"),
                new KeyValuePair<string, string>("Admin.NopStation.ZohoSalesIQ.Configuration.Fields.Script.Hint", "This field for script."),

                new KeyValuePair<string, string>("Admin.NopStation.ZohoSalesIQ.Configuration", "Zoho SalesIQ settings"),
                new KeyValuePair<string, string>("Admin.NopStation.ZohoSalesIQ.Menu.Configuration", "Configuration"),
                new KeyValuePair<string, string>("Admin.NopStation.ZohoSalesIQ.Menu.ZohoSalesIQ", "Zoho SalesIQ"),
            };
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.ZohoSalesIQ.Menu.ZohoSalesIQ"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(ZohoSalesIQPermissionProvider.ManageConfiguration))
            {
                var conItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.ZohoSalesIQ.Menu.Configuration"),
                    Url = "~/Admin/ZohoSalesIQ/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "ZohoSalesIQ.Configuration"
                };
                menuItem.ChildNodes.Add(conItem);
            }
            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/zoho-salesiq-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=zoho-salesiq",
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
