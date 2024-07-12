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
using NopStation.Plugin.Widgets.LuckyOrange.Components;

namespace NopStation.Plugin.Widgets.LuckyOrange
{
    public class LuckyOrangePlugin : BasePlugin, IWidgetPlugin, INopStationPlugin, IAdminMenuPlugin
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;
        private readonly INopStationCoreService _nopStationCoreService;

        #endregion

        #region Ctor

        public LuckyOrangePlugin(ILocalizationService localizationService,
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
            return _webHelper.GetStoreLocation() + "Admin/LuckyOrange/Configure";
        }

        public bool HideInWidgetList => false;
        
        public override async Task InstallAsync()
        {
            await this.InstallPluginAsync(new LuckyOrangePermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            //settings
            await _settingService.DeleteSettingAsync<LuckyOrangeSettings>();

            await this.UninstallPluginAsync(new LuckyOrangePermissionProvider());
            await base.UninstallAsync();
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            return new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Admin.NopStation.LuckyOrange.Configuration.Instructions", "<p>Collect Lucky orange tracking code or site id from <a href=\"https://app.luckyorange.com/settings/account/sites\">here</a>.</p>"),

                new KeyValuePair<string, string>("Admin.NopStation.LuckyOrange.Configuration.Fields.EnablePlugin", "Enable plugin"),
                new KeyValuePair<string, string>("Admin.NopStation.LuckyOrange.Configuration.Fields.EnablePlugin.Hint", "Determines whether the plugin is enabled or not."),
                new KeyValuePair<string, string>("Admin.NopStation.LuckyOrange.Configuration.Fields.SiteId", "Site id"),
                new KeyValuePair<string, string>("Admin.NopStation.LuckyOrange.Configuration.Fields.SiteId.Hint", "Enter Lucky orange site id."),
                new KeyValuePair<string, string>("Admin.NopStation.LuckyOrange.Configuration.Fields.TrackingCode", "Tracking code"),
                new KeyValuePair<string, string>("Admin.NopStation.LuckyOrange.Configuration.Fields.TrackingCode.Hint", "This field is for tracking code."),
                new KeyValuePair<string, string>("Admin.NopStation.LuckyOrange.Configuration.Fields.SettingMode", "Setting mode"),
                new KeyValuePair<string, string>("Admin.NopStation.LuckyOrange.Configuration.Fields.SettingMode.Hint", "Select setting mode."),

                new KeyValuePair<string, string>("Admin.NopStation.LuckyOrange.Configuration", "Lucky orange settings"),
                new KeyValuePair<string, string>("Admin.NopStation.LuckyOrange.Menu.Configuration", "Configuration"),
                new KeyValuePair<string, string>("Admin.NopStation.LuckyOrange.Menu.LuckyOrange", "Lucky orange"),
            };
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.LuckyOrange.Menu.LuckyOrange"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(LuckyOrangePermissionProvider.ManageConfiguration))
            {
                var conItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.LuckyOrange.Menu.Configuration"),
                    Url = "~/Admin/LuckyOrange/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "LuckyOrange.Configuration"
                };
                menuItem.ChildNodes.Add(conItem);
            }
            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/lucky-orange-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=lucky-orange",
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
            return typeof(LuckyOrangeViewComponent);
        }
    }
}
