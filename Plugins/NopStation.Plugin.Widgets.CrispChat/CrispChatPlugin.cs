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
using NopStation.Plugin.Widgets.CrispChat.Components;

namespace NopStation.Plugin.Widgets.CrispChat
{
    public class CrispChatPlugin : BasePlugin, IWidgetPlugin, INopStationPlugin, IAdminMenuPlugin
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;
        private readonly INopStationCoreService _nopStationCoreService;

        #endregion

        #region Ctor

        public CrispChatPlugin(ILocalizationService localizationService,
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
            return _webHelper.GetStoreLocation() + "Admin/CrispChat/Configure";
        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            return typeof(CrispChatViewComponent);
        }

        public bool HideInWidgetList => false;
        
        public override async Task InstallAsync()
        {
            //settings
            await _settingService.SaveSettingAsync(new CrispChatSettings
            {
                SettingMode = SettingMode.WebsiteId
            });

            await this.InstallPluginAsync(new CrispChatPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            //settings
            await _settingService.DeleteSettingAsync<CrispChatSettings>();

            await this.UninstallPluginAsync(new CrispChatPermissionProvider());
            await base.UninstallAsync();
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            return new List<KeyValuePair<string, string>>()
            {
                new("Admin.NopStation.CrispChat.Configuration.Instructions", "<p>Collect Crisp chat script or website id from <a href=\"https://crisp.chat/en/\">here</a>.</p>"),

                new("Admin.NopStation.CrispChat.Configuration.Fields.EnablePlugin", "Enable plugin"),
                new("Admin.NopStation.CrispChat.Configuration.Fields.EnablePlugin.Hint", "Determines whether the plugin is enabled or not."),
                new("Admin.NopStation.CrispChat.Configuration.Fields.WebsiteId", "Website id"),
                new("Admin.NopStation.CrispChat.Configuration.Fields.WebsiteId.Hint", "Enter Crisp chat website id."),
                new("Admin.NopStation.CrispChat.Configuration.Fields.Script", "Script"),
                new("Admin.NopStation.CrispChat.Configuration.Fields.Script.Hint", "This field for script."),
                new("Admin.NopStation.CrispChat.Configuration.Fields.SettingMode", "Setting mode"),
                new("Admin.NopStation.CrispChat.Configuration.Fields.SettingMode.Hint", "Select setting mode."),

                new("Admin.NopStation.CrispChat.Configuration", "Crisp chat settings"),
                new("Admin.NopStation.CrispChat.Menu.Configuration", "Configuration"),
                new("Admin.NopStation.CrispChat.Menu.CrispChat", "Crisp chat"),
            };
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.CrispChat.Menu.CrispChat"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(CrispChatPermissionProvider.ManageConfiguration))
            {
                var conItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.CrispChat.Menu.Configuration"),
                    Url = "~/Admin/CrispChat/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "CrispChat.Configuration"
                };
                menuItem.ChildNodes.Add(conItem);
            }
            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/crisp-chat-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=crisp-chat",
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
