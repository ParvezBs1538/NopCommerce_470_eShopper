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
using NopStation.Plugin.Widgets.Heap.Components;

namespace NopStation.Plugin.Widgets.Heap
{
    public class HeapPlugin : BasePlugin, IWidgetPlugin, INopStationPlugin, IAdminMenuPlugin
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;
        private readonly INopStationCoreService _nopStationCoreService;

        #endregion

        #region Ctor

        public HeapPlugin(ILocalizationService localizationService,
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
            return _webHelper.GetStoreLocation() + "Admin/Heap/Configure";
        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            return typeof(HeapViewComponent);
        }

        public bool HideInWidgetList => false;

        public override async Task InstallAsync()
        {
            await this.InstallPluginAsync(new HeapPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            //settings
            await _settingService.DeleteSettingAsync<HeapSettings>();

            await this.UninstallPluginAsync(new HeapPermissionProvider());
            await base.UninstallAsync();
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            return new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Admin.NopStation.Heap.Configuration.Instructions", "<p>Collect Heap script or app id from <a href=\"https://heapanalytics.com/app/manage/projects\">here</a>.</p>"),

                new KeyValuePair<string, string>("Admin.NopStation.Heap.Configuration.Fields.EnablePlugin", "Enable plugin"),
                new KeyValuePair<string, string>("Admin.NopStation.Heap.Configuration.Fields.EnablePlugin.Hint", "Determines whether the plugin is enabled or not."),
                new KeyValuePair<string, string>("Admin.NopStation.Heap.Configuration.Fields.AppId", "App id"),
                new KeyValuePair<string, string>("Admin.NopStation.Heap.Configuration.Fields.AppId.Hint", "Enter Heap app id."),
                new KeyValuePair<string, string>("Admin.NopStation.Heap.Configuration.Fields.Script", "Script"),
                new KeyValuePair<string, string>("Admin.NopStation.Heap.Configuration.Fields.Script.Hint", "This field is for script."),
                new KeyValuePair<string, string>("Admin.NopStation.Heap.Configuration.Fields.SettingMode", "Setting mode"),
                new KeyValuePair<string, string>("Admin.NopStation.Heap.Configuration.Fields.SettingMode.Hint", "Select setting mode."),

                new KeyValuePair<string, string>("Admin.NopStation.Heap.Configuration", "Heap settings"),
                new KeyValuePair<string, string>("Admin.NopStation.Heap.Menu.Configuration", "Configuration"),
                new KeyValuePair<string, string>("Admin.NopStation.Heap.Menu.Heap", "Heap"),
            };
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Heap.Menu.Heap"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(HeapPermissionProvider.ManageConfiguration))
            {
                var conItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Heap.Menu.Configuration"),
                    Url = "~/Admin/Heap/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "Heap.Configuration"
                };
                menuItem.ChildNodes.Add(conItem);
            }
            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/heap-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=heap",
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