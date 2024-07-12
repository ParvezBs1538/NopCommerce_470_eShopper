using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Helpers;
using NopStation.Plugin.Misc.Core.Services;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;

namespace NopStation.Plugin.Misc.MergeGuestOrder
{
    public class MergeGuestOrderPlugin : BasePlugin, IMiscPlugin, IAdminMenuPlugin, INopStationPlugin
    {
        #region Fields

        private readonly IWebHelper _webHelper;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;

        #endregion

        #region Ctor

        public MergeGuestOrderPlugin(IWebHelper webHelper,
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
            return $"{_webHelper.GetStoreLocation()}Admin/MergeGuestOrder/Configure";
        }
        
        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.MergeGuestOrder.Menu.MergeGuestOrder"),
                Visible = true,
                IconClass = "far fa-dot-circle"
            };

            if (await _permissionService.AuthorizeAsync(MergeGuestOrderPermissionProvider.ManageConfiguration))
            {
                var siteMap = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.MergeGuestOrder.Menu.Configuration"),
                    Url = "~/Admin/MergeGuestOrder/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "MergeGuestOrder.Configure"
                };
                menuItem.ChildNodes.Add(siteMap);
            }
            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/merge-guest-order-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=merge-guest-order",
                    Visible = true,
                    IconClass = "far fa-circle",
                    OpenUrlInNewTab = true
                };
                menuItem.ChildNodes.Add(documentation);
            }

            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menuItem, NopStationMenuType.Plugin);
        }

        public override async Task InstallAsync()
        {
            var settings = new MergeGuestOrderSettings()
            {
                EnablePlugin = true,
                AddNoteToOrderOnMerge = true,
                CheckEmailInAddressId = (int)CheckEmailInAddress.Both
            };
            await _settingService.SaveSettingAsync(settings);

            await this.InstallPluginAsync(new MergeGuestOrderPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await this.UninstallPluginAsync(new MergeGuestOrderPermissionProvider());
            await base.UninstallAsync();
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Admin.NopStation.MergeGuestOrder.Menu.MergeGuestOrder", "Merge guest order"),
                new KeyValuePair<string, string>("Admin.NopStation.MergeGuestOrder.Menu.Configuration", "Configuration"),

                new KeyValuePair<string, string>("Admin.NopStation.MergeGuestOrder.Configuration", "Merge guest order settings"),
                new KeyValuePair<string, string>("Admin.NopStation.MergeGuestOrder.Configuration.Fields.EnablePlugin", "Enable plugin"),
                new KeyValuePair<string, string>("Admin.NopStation.MergeGuestOrder.Configuration.Fields.EnablePlugin.Hint", "Determines whether the plugin is enabled or not."),
                new KeyValuePair<string, string>("Admin.NopStation.MergeGuestOrder.Configuration.Fields.AddNoteToOrderOnMerge", "Add note to order on merge"),
                new KeyValuePair<string, string>("Admin.NopStation.MergeGuestOrder.Configuration.Fields.AddNoteToOrderOnMerge.Hint", "Determines whether to add note to order on merge or not."),
                new KeyValuePair<string, string>("Admin.NopStation.MergeGuestOrder.Configuration.Fields.CheckEmailInAddress", "Check email in address"),
                new KeyValuePair<string, string>("Admin.NopStation.MergeGuestOrder.Configuration.Fields.CheckEmailInAddress.Hint", "Determines which address of an order (billing, shipping) should be checked for the email."),

                new KeyValuePair<string, string>("NopStation.MergeGuestOrder.OrderNote", "Guest order has been merged with a new customer (email: {0}).")
            };

            return list;
        }

        #endregion
    }
}
