using Nop.Core;
using Nop.Services.Plugins;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Web.Framework.Menu;
using System.Collections.Generic;
using Nop.Web.Framework.Infrastructure;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Misc.Core;
using Nop.Services.Security;
using System.Threading.Tasks;
using NopStation.Plugin.Widgets.ProductRequests.Components;
using System;

namespace NopStation.Plugin.Widgets.ProductRequests
{
    public class ProductRequestPlugin : BasePlugin, IWidgetPlugin, IAdminMenuPlugin, INopStationPlugin
    {
        #region Fields

        public bool HideInWidgetList => false;

        private readonly IWebHelper _webHelper;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;

        #endregion

        #region Ctor

        public ProductRequestPlugin(IWebHelper webHelper,
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
            return $"{_webHelper.GetStoreLocation()}Admin/ProductRequest/Configure";
        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            if (widgetZone == PublicWidgetZones.AccountNavigationAfter)
                return typeof(ProductRequestNavigationViewComponent);
            if (widgetZone == PublicWidgetZones.Footer)
                return typeof(ProductRequestFooterViewComponent);

            return typeof(ProductRequestHeaderMenuViewComponent);
        }

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string>
            {
                PublicWidgetZones.Footer,
                PublicWidgetZones.HeaderMenuAfter,
                PublicWidgetZones.MobHeaderMenuAfter,
                PublicWidgetZones.AccountNavigationAfter
            });
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.ProductRequests.Menu.ProductRequest"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(ProductRequestPermissionProvider.ManageProductRequests))
            {
                var listItem = new SiteMapNode()
                {
                    Title =await _localizationService.GetResourceAsync("Admin.NopStation.ProductRequests.Menu.ProductRequests"),
                    Url = "~/Admin/ProductRequest/List",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "ProductRequests"
                };
                menuItem.ChildNodes.Add(listItem);

                var configItem = new SiteMapNode()
                {
                    Title =await _localizationService.GetResourceAsync("Admin.NopStation.ProductRequests.Menu.Configuration"),
                    Url = "~/Admin/ProductRequest/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "ProductRequests.Configuration"
                };
                menuItem.ChildNodes.Add(configItem);
            }

            var documentation = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                Url = "https://www.nop-station.com/product-request-documentation?utm_source=admin-panel?utm_source=admin-panel&utm_medium=products&utm_campaign=product-request",
                Visible = true,
                IconClass = "far fa-circle",
                OpenUrlInNewTab = true
            };
            menuItem.ChildNodes.Add(documentation);

            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menuItem, NopStationMenuType.Plugin);
        }

        public override async Task InstallAsync()
        {
            await _settingService.SaveSettingAsync(new ProductRequestSettings
            {
                IncludeInTopMenu = true,
                IncludeInFooter = true,
                FooterElementSelector = ".footer-block.customer-service .list",
                AllowedCustomerRolesIds = new List<int> { 1, 2, 3 },
                MinimumProductRequestCreateInterval = 60
            });

            await this.InstallPluginAsync(new ProductRequestPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await this.UninstallPluginAsync(new ProductRequestPermissionProvider());
            await base.UninstallAsync();
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Admin.NopStation.ProductRequests.Menu.ProductRequest", "Product request"),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRequests.Menu.Configuration", "Configuration"),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRequests.Menu.ProductRequests", "Product requests"),

                new KeyValuePair<string, string>("Admin.NopStation.ProductRequests.Configuration", "Product request settings"),

                new KeyValuePair<string, string>("Admin.NopStation.ProductRequests.ProductRequests.List", "Product requests"),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRequests.EditDetails", "Edit request details"),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRequests.BackToList", "back to request list"),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRequests.Updated", "Product request has been updated successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRequests.Deleted", "Product request has been deleted successfully."),

                new KeyValuePair<string, string>("Admin.NopStation.ProductRequests.Configuration.Fields.AllowedCustomerRolesIds", "Allowed customer roles"),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRequests.Configuration.Fields.AllowedCustomerRolesIds.Hint", "Select allowed customer roles."),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRequests.Configuration.Fields.IncludeInTopMenu", "Include in top menu"),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRequests.Configuration.Fields.IncludeInTopMenu.Hint", "Determines whether to include in top menu or not."),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRequests.Configuration.Fields.IncludeInFooter", "Include in footer"),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRequests.Configuration.Fields.IncludeInFooter.Hint", "Determines whether to include in footer or not."),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRequests.Configuration.Fields.FooterElementSelector", "Footer element selector"),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRequests.Configuration.Fields.FooterElementSelector.Hint", "Enter footer element selector."),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRequests.Configuration.Fields.LinkRequired", "Link required"),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRequests.Configuration.Fields.LinkRequired.Hint", "Determines whether link is required or not."),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRequests.Configuration.Fields.DescriptionRequired", "Description required"),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRequests.Configuration.Fields.DescriptionRequired.Hint", "Determines whether description is required or not."),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRequests.Configuration.Fields.MinimumProductRequestCreateInterval", "Minimum product request create interval"),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRequests.Configuration.Fields.MinimumProductRequestCreateInterval.Hint", "Minimum product request create interval."),

                new KeyValuePair<string, string>("Admin.NopStation.ProductRequests.ProductRequests.Fields.Name", "Name"),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRequests.ProductRequests.Fields.Name.Hint", "The product request name."),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRequests.ProductRequests.Fields.Link", "Link"),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRequests.ProductRequests.Fields.Link.Hint", "The product request link."),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRequests.ProductRequests.Fields.Description", "Description"),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRequests.ProductRequests.Fields.Description.Hint", "The product request description."),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRequests.ProductRequests.Fields.AdminComment", "Admin comment"),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRequests.ProductRequests.Fields.AdminComment.Hint", "The product request admin comment."),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRequests.ProductRequests.Fields.Store", "Store"),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRequests.ProductRequests.Fields.Store.Hint", "The product request store."),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRequests.ProductRequests.Fields.Customer", "Customer"),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRequests.ProductRequests.Fields.Customer.Hint", "The product request customer."),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRequests.ProductRequests.Fields.CreatedOn", "Created on"),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRequests.ProductRequests.Fields.CreatedOn.Hint", "The product request create date."),

                new KeyValuePair<string, string>("Admin.NopStation.ProductRequests.ProductRequests.Fields.Name.Required", "The name field is required."),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRequests.ProductRequests.Fields.Description.Required", "The description field is required."),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRequests.ProductRequests.Fields.Link.Required", "The link field is required."),

                new KeyValuePair<string, string>("NopStation.ProductRequests.ProductRequests.Fields.Name.Required", "The name field is required."),
                new KeyValuePair<string, string>("NopStation.ProductRequests.ProductRequests.Fields.Description.Required", "The description field is required."),
                new KeyValuePair<string, string>("NopStation.ProductRequests.ProductRequests.Fields.Link.Required", "The link field is required."),

                new KeyValuePair<string, string>("Admin.NopStation.ProductRequests.ProductRequests.Fields.Name.MaxLength", "Maximum length 200."),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRequests.ProductRequests.Fields.Description.MaxLength", "Maximum length 400."),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRequests.ProductRequests.Fields.Link.MaxLength", "Maximum length 500."),
                new KeyValuePair<string, string>("NopStation.ProductRequests.ProductRequests.Fields.Name.MaxLength", "Maximum length 200."),
                new KeyValuePair<string, string>("NopStation.ProductRequests.ProductRequests.Fields.Description.MaxLength", "Maximum length 400."),
                new KeyValuePair<string, string>("NopStation.ProductRequests.ProductRequests.Fields.Link.MaxLength", "Maximum length 500."),

                new KeyValuePair<string, string>("NopStation.ProductRequests.ProductRequests.Fields.Name", "Name"),
                new KeyValuePair<string, string>("NopStation.ProductRequests.ProductRequests.Fields.Link", "Link"),
                new KeyValuePair<string, string>("NopStation.ProductRequests.ProductRequests.Fields.Description", "Description"),
                new KeyValuePair<string, string>("NopStation.ProductRequests.ProductRequests.Fields.CreatedOn", "Created On"),
                new KeyValuePair<string, string>("NopStation.ProductRequests.ProductRequests.Fields.AdminComment", "Admin Comment"),

                new KeyValuePair<string, string>("NopStation.ProductRequests.Account.ProductRequestDetails", "Product request details"),
                new KeyValuePair<string, string>("NopStation.ProductRequests.ProductRequest", "Product request"),
                new KeyValuePair<string, string>("NopStation.ProductRequests.Account.CreateNewRequest", "Create new request"),
                new KeyValuePair<string, string>("NopStation.ProductRequests.ProductRequests.SubmitButton", "Submit"),
                new KeyValuePair<string, string>("NopStation.ProductRequests.ProductRequests.View", "View"),
                new KeyValuePair<string, string>("NopStation.ProductRequests.ProductRequests.ListIsEmpty", "List is empty"),
                new KeyValuePair<string, string>("NopStation.ProductRequests.Account.MyProductRequests", "My product requests"),
                new KeyValuePair<string, string>("NopStation.ProductRequests.Navigation.MyProductRequests", "My product requests"),
                new KeyValuePair<string, string>("NopStation.ProductRequests.ProductRequests.MinProductRequestCreateInterval", "Please wait several seconds before creating a new product request (already created another product request several seconds ago)."),

                new KeyValuePair<string, string>("Admin.NopStation.ProductRequests.ProductRequests.List.SearchName", "Name"),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRequests.ProductRequests.List.SearchName.Hint", "The search name."),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRequests.ProductRequests.List.SearchStore", "Store"),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRequests.ProductRequests.List.SearchStore.Hint", "The search store."),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRequests.ProductRequests.List.SearchCustomerEmail", "Customer email"),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRequests.ProductRequests.List.SearchCustomerEmail.Hint", "The search customer email.")
            };

            return list;
        }

        #endregion
    }
}
