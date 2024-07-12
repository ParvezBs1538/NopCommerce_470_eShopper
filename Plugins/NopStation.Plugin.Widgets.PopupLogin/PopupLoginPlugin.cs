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
using NopStation.Plugin.Widgets.PopupLogin.Components;

namespace NopStation.Plugin.Widgets.PopupLogin
{
    public class PopupLoginPlugin : BasePlugin, IAdminMenuPlugin, INopStationPlugin, IWidgetPlugin
    {
        #region Fields

        private readonly IWebHelper _webHelper;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;

        public bool HideInWidgetList => false;

        #endregion

        #region Ctor

        public PopupLoginPlugin(IWebHelper webHelper,
            INopStationCoreService nopStationCoreService,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            ISettingService settingService)
        {
            _webHelper = webHelper;
            _nopStationCoreService = nopStationCoreService;
            _localizationService = localizationService;
            _permissionService = permissionService;
            _settingService = settingService;
        }

        #endregion

        #region Methods

        public override string GetConfigurationPageUrl()
        {
            return _webHelper.GetStoreLocation() + "Admin/PopupLogin/Configure";
        }

        public override async Task InstallAsync()
        {
            var settings = new PopupLoginSettings()
            { 
                EnablePopupLogin = true,
                LoginUrlElementSelector = ".header-links li .ico-login"
            };
            await _settingService.SaveSettingAsync(settings);

            await this.InstallPluginAsync(new PopupLoginPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await this.UninstallPluginAsync(new PopupLoginPermissionProvider());
            await base.UninstallAsync();
        }
        
        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.PopupLogin.Menu.PopupLogin"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(PopupLoginPermissionProvider.ManagePopupLogin))
            {
                var configItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.PopupLogin.Menu.Configuration"),
                    Url = "~/Admin/PopupLogin/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "PopupLogin.Configuration"
                };
                menuItem.ChildNodes.Add(configItem);
            }

            var documentation = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                Url = "https://www.nop-station.com/popup-login-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=picture-zoom",
                Visible = true,
                IconClass = "far fa-circle",
                OpenUrlInNewTab = true
            };
            menuItem.ChildNodes.Add(documentation);

            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menuItem, NopStationMenuType.Plugin);
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>
            {
                new("Admin.NopStation.PopupLogin.Menu.PopupLogin", "Popup login"),
                new("Admin.NopStation.PopupLogin.Menu.Configuration", "Configuration"),
                new("Admin.NopStation.PopupLogin.Configuration.Fields.EnablePopupLogin", "Enable popup login"),
                new("Admin.NopStation.PopupLogin.Configuration.Fields.EnablePopupLogin.Hint", "Check to enable popup login."),
                new("Admin.NopStation.PopupLogin.Configuration.Fields.LoginUrlElementSelector", "Login url element selector"),
                new("Admin.NopStation.PopupLogin.Configuration.Fields.LoginUrlElementSelector.Hint", "Enter login url element selector (i.e. .header-links li .ico-login)."),
                new("Admin.NopStation.PopupLogin.Configuration", "Popup login settings")
            };

            return list;
        }

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string> { PublicWidgetZones.Footer });
        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            return typeof(PopupLoginViewComponent);
        }

        #endregion
    }
}