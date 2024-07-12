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
using NopStation.Plugin.Widgets.GoogleTagManager.Components;

namespace NopStation.Plugin.Widgets.GoogleTagManager
{
    public class GoogleTagManagerPlugin : BasePlugin, IWidgetPlugin, IAdminMenuPlugin, INopStationPlugin
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly IPermissionService _permissionService;

        #endregion

        #region Ctor

        public GoogleTagManagerPlugin(ILocalizationService localizationService,
            INopStationCoreService nopStationCoreService,
            ISettingService settingService,
            IPermissionService permissionService,
            IWebHelper webHelper)
        {
            _localizationService = localizationService;
            _nopStationCoreService = nopStationCoreService;
            _settingService = settingService;
            _permissionService = permissionService;
            _webHelper = webHelper;
        }

        #endregion

        #region Methods

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string> {
                PublicWidgetZones.HeadHtmlTag,
                PublicWidgetZones.BodyStartHtmlTagAfter
            });
        }

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/GoogleTagManager/Configure";
        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            return typeof(GoogleTagManagerViewComponent);
        }

        public bool HideInWidgetList => false;

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menu = new SiteMapNode()
            {
                Visible = true,
                IconClass = "far fa-dot-circle",
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.GoogleTagManager.Menu.GoogleTagManager")
            };

            if (await _permissionService.AuthorizeAsync(GoogleTagManagerPermissionProvider.ManageConfiguration))
            {
                var settings = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.GoogleTagManager.Menu.Configuration"),
                    Url = $"{_webHelper.GetStoreLocation()}Admin/GoogleTagManager/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "GoogleTagManager.Configuration"
                };
                menu.ChildNodes.Add(settings);
            }
            if (await _permissionService.AuthorizeAsync(GoogleTagManagerPermissionProvider.ManageExportFile))
            {
                var settings = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.GoogleTagManager.Menu.ExportFile"),
                    Url = $"{_webHelper.GetStoreLocation()}Admin/GoogleTagManager/ExportFile",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "GoogleTagManager.ExportFile"
                };
                menu.ChildNodes.Add(settings);
            }

            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/google-tag-manager-documentation?utm_source=admin-panel?utm_source=admin-panel&utm_medium=gtm&utm_campaign=gtm",
                    Visible = true,
                    IconClass = "far fa-circle",
                    OpenUrlInNewTab = true
                };
                menu.ChildNodes.Add(documentation);
            }

            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menu, NopStationMenuType.Plugin);
        }

        #region Install & UnInstall

        public override async Task InstallAsync()
        {
            var googleTagManagerConfigurationSettings = new GoogleTagManagerSettings
            {
                GTMContainerId = "GTM-XXXXXX"
            };
            await _settingService.SaveSettingAsync(googleTagManagerConfigurationSettings);
            await this.InstallPluginAsync(new GoogleTagManagerPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await this.UninstallPluginAsync(new GoogleTagManagerPermissionProvider());
            await base.UninstallAsync();
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Admin.NopStation.GoogleTagManager.Menu.GoogleTagManager", "Google Tag Manager"),
                new KeyValuePair<string, string>("Admin.NopStation.GoogleTagManager.Menu.Configuration", "Configuration"),
                new KeyValuePair<string, string>("Admin.NopStation.GoogleTagManager.Menu.ExportFile", "Export File"),

                new KeyValuePair<string, string>("Admin.NopStation.GoogleTagManager.Configuration.Fields.IsEnable", "Enable plugin"),
                new KeyValuePair<string, string>("Admin.NopStation.GoogleTagManager.Configuration.Fields.IsEnable.Hint", "Enable this plugin."),
                new KeyValuePair<string, string>("Admin.NopStation.GoogleTagManager.Configuration.Fields.GTMContainerId", "Google Tag Manager Id"),
                new KeyValuePair<string, string>("Admin.NopStation.GoogleTagManager.Configuration.Fields.GTMContainerId.Hint", "Give Google Tag Manager Container Id such as GTM-XXXXXX"),

                new KeyValuePair<string, string>("Admin.NopStation.GoogleTagManager.ExportFileInformation.Fields.GAContainerId", "Google analytics ID"),
                new KeyValuePair<string, string>("Admin.NopStation.GoogleTagManager.ExportFileInformation.Fields.GAContainerId.Hint", "Give Google analytics 4 ID."),

                new KeyValuePair<string, string>("Admin.NopStation.GoogleTagManager.Configuration.GTMContainerId.Required", "This field is required."),

                new KeyValuePair<string, string>("Admin.NopStation.GoogleTagManager.Configuration", "Google Tag Manager settings"),
                new KeyValuePair<string, string>("Admin.NopStation.GoogleTagManager.ExportFile", "Google Tag Manager Export File")
            };

            return list;
        }

        #endregion

        #endregion
    }
}