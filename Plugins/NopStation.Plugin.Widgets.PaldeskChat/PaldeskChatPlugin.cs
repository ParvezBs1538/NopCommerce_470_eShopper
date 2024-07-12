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
using NopStation.Plugin.Widgets.PaldeskChat.Components;
using System;

namespace NopStation.Plugin.Widgets.PaldeskChat
{
    public class PaldeskChatPlugin : BasePlugin, IWidgetPlugin, INopStationPlugin, IAdminMenuPlugin
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;
        private readonly INopStationCoreService _nopStationCoreService;

        #endregion

        #region Ctor

        public PaldeskChatPlugin(ILocalizationService localizationService,
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
            return _webHelper.GetStoreLocation() + "Admin/PaldeskChat/Configure";
        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            return typeof(PaldeskChatViewComponent);
        }

        public bool HideInWidgetList => false;
        
        public override async Task InstallAsync()
        {
            //settings
            await _settingService.SaveSettingAsync(new PaldeskChatSettings
            {
                SettingMode = SettingMode.Key,
                ConfigureWithCustomerDataIfLoggedIn = true
            });

            await this.InstallPluginAsync(new PaldeskChatPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            //settings
            await _settingService.DeleteSettingAsync<PaldeskChatSettings>();

            await this.UninstallPluginAsync(new PaldeskChatPermissionProvider());
            await base.UninstallAsync();
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            return new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Admin.NopStation.PaldeskChat.Configuration.Instructions", "<p>Collect paldesk chat script or key from <a href=\"https://paldesk.io/desk/dashboard/admin/widget\">here</a>.</p>"),

                new KeyValuePair<string, string>("Admin.NopStation.PaldeskChat.Configuration.Fields.EnablePlugin", "Enable plugin"),
                new KeyValuePair<string, string>("Admin.NopStation.PaldeskChat.Configuration.Fields.EnablePlugin.Hint", "Determines whether the plugin is enabled or not."),
                new KeyValuePair<string, string>("Admin.NopStation.PaldeskChat.Configuration.Fields.Key", "Key"),
                new KeyValuePair<string, string>("Admin.NopStation.PaldeskChat.Configuration.Fields.Key.Hint", "Enter paldesk chat key."),
                new KeyValuePair<string, string>("Admin.NopStation.PaldeskChat.Configuration.Fields.Script", "Script"),
                new KeyValuePair<string, string>("Admin.NopStation.PaldeskChat.Configuration.Fields.Script.Hint", "This field for script."),
                new KeyValuePair<string, string>("Admin.NopStation.PaldeskChat.Configuration.Fields.SettingMode", "Setting mode"),
                new KeyValuePair<string, string>("Admin.NopStation.PaldeskChat.Configuration.Fields.SettingMode.Hint", "Select setting mode."),
                new KeyValuePair<string, string>("Admin.NopStation.PaldeskChat.Configuration.Fields.ConfigureWithCustomerDataIfLoggedIn", "Configure with customer data if logged in"),
                new KeyValuePair<string, string>("Admin.NopStation.PaldeskChat.Configuration.Fields.ConfigureWithCustomerDataIfLoggedIn.Hint", "Configure with customer data if the customer logged in."),

                new KeyValuePair<string, string>("Admin.NopStation.PaldeskChat.Configuration", "Paldesk chat settings"),
                new KeyValuePair<string, string>("Admin.NopStation.PaldeskChat.Menu.Configuration", "Configuration"),
                new KeyValuePair<string, string>("Admin.NopStation.PaldeskChat.Menu.PaldeskChat", "Paldesk chat"),
            };
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.PaldeskChat.Menu.PaldeskChat"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(PaldeskChatPermissionProvider.ManageConfiguration))
            {
                var conItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.PaldeskChat.Menu.Configuration"),
                    Url = "~/Admin/PaldeskChat/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "PaldeskChat.Configuration"
                };
                menuItem.ChildNodes.Add(conItem);
            }

            var documentation = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                Url = "https://www.nop-station.com/paldesk-chat-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=paldesk-chat",
                Visible = true,
                IconClass = "far fa-circle",
                OpenUrlInNewTab = true
            };
            menuItem.ChildNodes.Add(documentation);

            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menuItem, NopStationMenuType.Plugin);
        }
    }
}
