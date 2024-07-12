using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;

namespace NopStation.Plugin.Widgets.CrawlerManager
{
    public class CrawlerManager : BasePlugin, IAdminMenuPlugin, INopStationPlugin
    {
        #region Properties

        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly IPermissionService _permissionService;

        #endregion

        #region Ctors

        public CrawlerManager(ILocalizationService localizationService,
            IWebHelper webHelper,
            INopStationCoreService nopStationCoreService,
            IPermissionService permissionService)
        {
            _localizationService = localizationService;
            _webHelper = webHelper;
            _nopStationCoreService = nopStationCoreService;
            _permissionService = permissionService;
        }

        #endregion

        #region Methods

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/CrawlerManager/Configure";
        }

        public bool HideInWidgetList => false;

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("NopStation.Plugin.Widgets.CrawlerManager.Menu.CrawlerManager"),
                Visible = true,
                IconClass = "far fa-dot-circle"
            };

            if (await _permissionService.AuthorizeAsync(CrawlerManagerPermissionProvider.ManageCrawlerManager))
            {
                var configItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("NopStation.Plugin.Widgets.CrawlerManager.Menu.Configuration"),
                    Url = "~/Admin/CrawlerManager/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "CrawlerManager.Configuration"
                };
                menuItem.ChildNodes.Add(configItem);

                var guestList = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("NopStation.Plugin.Widgets.CrawlerManager.Menu.GuestCustomerList"),
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "CrawlerManager.GuestList",
                    Url = "~/Admin/CrawlerManager/GuestList"
                };
                menuItem.ChildNodes.Add(guestList);

                //var customersItem = rootNode.ChildNodes.FirstOrDefault(node => node.SystemName.Equals("Customers"));
                //if (customersItem is not null)
                //{
                //    var guestList2 = new SiteMapNode()
                //    {
                //        Title = await _localizationService.GetResourceAsync("NopStation.Plugin.Widgets.CrawlerManager.Menu.GuestCustomerList"),
                //        Visible = true,
                //        IconClass = "far fa-dot-circle",
                //        SystemName = "CrawlerManager.GuestList",
                //        ControllerName = "CrawlerManager",
                //        ActionName = "GuestList",
                //        RouteValues = new RouteValueDictionary { { "area", AreaNames.Admin } }
                //    };
                //    customersItem.ChildNodes.Insert(customersItem.ChildNodes.Count, guestList2);
                //}

                var crawlerList = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("NopStation.Plugin.Widgets.CrawlerManager.Menu.CrawlerManagerList"),
                    Visible = true,
                    IconClass = "far fa-circle",
                    Url = "~/Admin/CrawlerManager/List",
                    SystemName = "CrawlerManager.List"
                };
                menuItem.ChildNodes.Add(crawlerList);

                await _nopStationCoreService.ManageSiteMapAsync(rootNode, menuItem, NopStationMenuType.Plugin);
            }
            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/crawler-manager-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=crawler-manager",
                    Visible = true,
                    IconClass = "far fa-circle",
                    OpenUrlInNewTab = true
                };
                menuItem.ChildNodes.Add(documentation);
            }
        }

        public override async Task InstallAsync()
        {
            await this.InstallPluginAsync(new CrawlerManagerPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await this.UninstallPluginAsync(new CrawlerManagerPermissionProvider());
            await base.UninstallAsync();
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            return new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("NopStation.Plugin.Widgets.CrawlerManager.Menu.CrawlerManager", "Crawler manager"),
                new KeyValuePair<string, string>("NopStation.Plugin.Widgets.CrawlerManager.Menu.GuestCustomerList", "Online guest customers"),
                new KeyValuePair<string, string>("NopStation.Plugin.Widgets.CrawlerManager.Menu.CrawlerManagerList", "Crawler list"),
                new KeyValuePair<string, string>("NopStation.Plugin.Widgets.CrawlerManager.Menu.Configuration", "Configuration"),
                new KeyValuePair<string, string>("NopStation.Plugin.Widgets.CrawlerManager.Menu.Documentation", "Documentation"),

                new KeyValuePair<string, string>("NopStation.Plugin.Widgets.CrawlerManager.Fields.IsEnabled", "Is enabled?"),
                new KeyValuePair<string, string>("NopStation.Plugin.Widgets.CrawlerManager.Fields.IsEnabled.Hint", "Determine is enabled or not. Enabling it will add the user agent in the admin comment"),
                new KeyValuePair<string, string>("NopStation.Plugin.Widgets.CrawlerManager.Fields.CrawlerInfo", "Crawler info"),
                new KeyValuePair<string, string>("NopStation.Plugin.Widgets.CrawlerManager.Fields.IPAddress", "IP address"),
                new KeyValuePair<string, string>("NopStation.Plugin.Widgets.CrawlerManager.Fields.Location", "Location"),
                new KeyValuePair<string, string>("NopStation.Plugin.Widgets.CrawlerManager.Fields.AddedOnUtc", "Added on"),
                new KeyValuePair<string, string>("NopStation.Plugin.Widgets.CrawlerManager.Fields.AddedBy", "Added by"),
                new KeyValuePair<string, string>("NopStation.Plugin.Widgets.CrawlerManager.AddCrawler", "Add crawler"),
                new KeyValuePair<string, string>("NopStation.Plugin.Widgets.CrawlerManager.GuestCustomers", "Online guest customers"),
                new KeyValuePair<string, string>("NopStation.Plugin.Widgets.CrawlerManager.error.exist", "Crawler already exist in the file"),
                new KeyValuePair<string, string>("NopStation.Plugin.Widgets.CrawlerManager.error.crawlerAdd", "Failed to add crawler"),
                new KeyValuePair<string, string>("NopStation.Plugin.Widgets.CrawlerManager.success.crawlerAdd", "Add to crawler list successful"),
                new KeyValuePair<string, string>("NopStation.Plugin.Widgets.CrawlerManager.ModalConfirmation", "Adding the Guest customer to the crawler list will not be undone."),
                new KeyValuePair<string, string>("NopStation.Plugin.Widgets.CrawlerManager.LogError.AddFailed", "(Crawler manager) Failed to add crawler to the list.")
            };
        }

        #endregion
    }
}
