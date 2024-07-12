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
using NopStation.Plugin.Misc.AdminReportExporter.Components;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;

namespace NopStation.Plugin.Misc.AdminReportExporter
{
    public class AdminReportExporterPlugin : BasePlugin, IWidgetPlugin, INopStationPlugin, IAdminMenuPlugin
    {
        #region Fields
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly IPermissionService _permissionService;
        private readonly INopStationCoreService _nopStationCoreService;

        public bool HideInWidgetList => false;

        #endregion

        #region Ctor

        public AdminReportExporterPlugin(
            ILocalizationService localizationService,
            ISettingService settingService,
            IWebHelper webHelper,
            IPermissionService permissionService,
            INopStationCoreService nopStationCoreService)
        {
            _localizationService = localizationService;
            _settingService = settingService;
            _webHelper = webHelper;
            _permissionService = permissionService;
            _nopStationCoreService = nopStationCoreService;
        }

        #endregion

        #region Methods

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/AdminReportExporter/Configure";
        }

        public override async Task InstallAsync()
        {
            await this.InstallPluginAsync(new AdminReportExporterPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await _settingService.DeleteSettingAsync<AdminReportExporterSettings>();

            await this.UninstallPluginAsync(new AdminReportExporterPermissionProvider());
            await base.UninstallAsync();
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menu = new SiteMapNode()
            {
                Visible = true,
                IconClass = "far fa-dot-circle",
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.AdminReportExporter.Menu.AdminReportExporter")
            };

            if (await _permissionService.AuthorizeAsync(AdminReportExporterPermissionProvider.ManageAdminReportExporter))
            {
                var settings = new SiteMapNode()
                {
                    Visible = true,
                    IconClass = "far fa-circle",
                    Url = $"{_webHelper.GetStoreLocation()}Admin/AdminReportExporter/Configure",
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.AdminReportExporter.Menu.Configuration"),
                    SystemName = "AdminReportExporter.Configuration"
                };
                menu.ChildNodes.Add(settings);
            }

            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/admin-report-exporter-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=admin-report-exporter",
                    Visible = true,
                    IconClass = "far fa-circle",
                    OpenUrlInNewTab = true
                };
                menu.ChildNodes.Add(documentation);
            }

            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menu, NopStationMenuType.Plugin);
        }

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            var list = new List<string>
            {
                AdminWidgetZones.MenuBefore
            };

            return Task.FromResult<IList<string>>(list);
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>
            {
                new("Admin.NopStation.AdminReportExporter.Menu.AdminReportExporter", "Admin report exporter"),
                new("Admin.NopStation.AdminReportExporter.Menu.Configuration", "Configuration"),
                new("Admin.NopStation.AdminReportExporter.Configuration.Fields.EnablePlugin", "Enable plugin"),
                new("Admin.NopStation.AdminReportExporter.Configuration.Title", "Configuration"),
                new("Admin.NopStation.AdminReportExporter.Configuration.Fields.EnablePlugin.Hint", "Click here to enable plugin."),
                new("Admin.NopStation.AdminReportExporter.Export", "Export")
            };

            return list;
        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            return typeof(MenuReportViewComponent);
        }

        #endregion
    }
}