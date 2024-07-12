using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Widgets.AffiliateStation.Extensions;
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
using NopStation.Plugin.Widgets.AffiliateStation.Components;
using NopStation.Plugin.Widgets.AffiliateStation.Areas.Admin.Components;

namespace NopStation.Plugin.Widgets.AffiliateStation
{
    public partial class AffiliateStationPlugin : BasePlugin, IWidgetPlugin, IAdminMenuPlugin, INopStationPlugin
    {
        #region Fields

        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;
        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly INopStationCoreService _nopStationCoreService;

        public bool HideInWidgetList => false;

        #endregion

        #region Ctor

        public AffiliateStationPlugin(ISettingService settingService,
            IPermissionService permissionService,
            ILocalizationService localizationService,
            IWebHelper webHelper,
            INopStationCoreService nopStationCoreService)
        {
            _settingService = settingService;
            _permissionService = permissionService;
            _localizationService = localizationService;
            _webHelper = webHelper;
            _nopStationCoreService = nopStationCoreService;
        }

        #endregion

        #region Methods

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/AffiliateStation/Configure";
        }

        public override async Task InstallAsync()
        {
            var settings = new AffiliateStationSettings()
            {
                AffiliatePageOrderPageSize = 10,
                UseDefaultCommissionIfNotSetOnCatalog = true,
                UsePercentage = true
            };
            await _settingService.SaveSettingAsync(settings);

            await this.InstallPluginAsync(new AffiliateStationPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await this.UninstallPluginAsync(new AffiliateStationPermissionProvider());
            await base.UninstallAsync();
        }

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string> { PublicWidgetZones.AccountNavigationAfter, AdminWidgetZones.AffiliateDetailsBlock });
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menu = new SiteMapNode()
            {
                Visible = true,
                IconClass = "far fa-dot-circle",
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.AffiliateStation.Menu.AffiliateStation")
            };

            if (await _permissionService.AuthorizeAsync(AffiliateStationPermissionProvider.ManageConfiguration))
            {
                var settings = new SiteMapNode()
                {
                    Visible = true,
                    IconClass = "far fa-circle",
                    Url = "~/Admin/AffiliateStation/Configure",
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.AffiliateStation.Menu.Configuration"),
                    SystemName = "AffiliateStation.Configuration"
                };
                menu.ChildNodes.Add(settings);
            }

            if (await _permissionService.AuthorizeAsync(AffiliateStationPermissionProvider.ManageAffiliateCustomer))
            {
                var userCommission = new SiteMapNode()
                {
                    Visible = true,
                    IconClass = "far fa-circle",
                    Url = "~/Admin/AffiliateCustomer/List",
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.AffiliateStation.Menu.AffiliateCustomer"),
                    SystemName = "AffiliateCustomers.List"
                };
                menu.ChildNodes.Add(userCommission);
            }

            if (await _permissionService.AuthorizeAsync(AffiliateStationPermissionProvider.ManageCatalogCommission))
            {
                var catalogCommission = new SiteMapNode()
                {
                    Visible = true,
                    IconClass = "far fa-circle",
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.AffiliateStation.Menu.CatalogCommission"),
                    SystemName = "CatalogCommissions.List"
                };
                var productCommission = new SiteMapNode()
                {
                    Visible = true,
                    IconClass = "far fa-circle",
                    Url = "~/Admin/CatalogCommission/ProductList",
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.AffiliateStation.Menu.ProductCommission"),
                    SystemName = "ProductCommissions.List"
                };
                catalogCommission.ChildNodes.Add(productCommission);
                var categoryCommission = new SiteMapNode()
                {
                    Visible = true,
                    IconClass = "far fa-circle",
                    Url = "~/Admin/CatalogCommission/CategoryList",
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.AffiliateStation.Menu.CategoryCommission"),
                    SystemName = "CategoryCommissions.List"
                };
                catalogCommission.ChildNodes.Add(categoryCommission);
                var manufacturerCommission = new SiteMapNode()
                {
                    Visible = true,
                    IconClass = "far fa-circle",
                    Url = "~/Admin/CatalogCommission/ManufacturerList",
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.AffiliateStation.Menu.ManufacturerCommission"),
                    SystemName = "ManufacturerCommissions.List"
                };
                catalogCommission.ChildNodes.Add(manufacturerCommission);
                menu.ChildNodes.Add(catalogCommission);
            }

            if (await _permissionService.AuthorizeAsync(AffiliateStationPermissionProvider.ManageOrderCommission))
            {
                var order = new SiteMapNode()
                {
                    Visible = true,
                    IconClass = "far fa-circle",
                    Url = "~/Admin/OrderCommission/List",
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.AffiliateStation.Menu.OrderCommission"),
                    SystemName = "OrderCommissions.List"
                };
                menu.ChildNodes.Add(order);
            }
            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/affiliate-station-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=affiliate-station",
                    Visible = true,
                    IconClass = "far fa-circle",
                    OpenUrlInNewTab = true
                };
                menu.ChildNodes.Add(documentation);
            }
                
            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menu, NopStationMenuType.Plugin);
        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            if (widgetZone == PublicWidgetZones.AccountNavigationAfter)
                return typeof(AffiliateStationViewComponent);

            return typeof(AffiliateStationAdminViewComponent);

        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.Menu.AffiliateStation", "Affiliate station"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.Menu.Configuration", "Configuration"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.Menu.AffiliateCustomer", "Affiliate customers"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.Menu.CatalogCommission", "Catalog commission"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.Menu.CategoryCommission", "Category"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.Menu.ProductCommission", "Product"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.Menu.ManufacturerCommission", "Manufacturer"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.Menu.OrderCommission", "Order commission"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.Configuration", "Affiliate station settings"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.AffiliateCustomers.List", "Affiliate customers"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.Affiliates.Customer", "Customer details"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.AffiliateCustomers.BackToList", "back to affiliate customer list"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.AffiliateCustomers.Updated", "Affiliate customer updated successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.CatalogCommissions.CategoryList", "Category commission list"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.CatalogCommissions.ProductList", "Product commission list"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.CatalogCommissions.ManufacturerList", "Manufacturer commission list"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.CatalogCommissions.ProductEdit", "Edit product commission details"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.CatalogCommissions.CategoryEdit", "Edit category commission details"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.CatalogCommissions.ManufacturerEdit", "Edit manufacturer commission details"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.CatalogCommissions.BackToProductList", "back to product list"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.CatalogCommissions.BackToCategoryList", "back to category list"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.CatalogCommissions.BackToManufacturerList", "back to manufacturer list"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.CatalogCommissions.Updated", "Catalog commission updated successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.OrderCommissions.Edit", "Edit order commission details"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.OrderCommissions.BackToList", "back to commission list"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.OrderCommissions.Updated", "Order commission updated successfully."),

                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.AffiliateCustomers.Fields.Affiliate", "Affiliate"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.AffiliateCustomers.Fields.Affiliate.Hint", "Affiliate"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.AffiliateCustomers.Fields.Customer", "Customer"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.AffiliateCustomers.Fields.Customer.Hint", "Customer"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.AffiliateCustomers.Fields.CustomerFullName", "Customer full name"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.AffiliateCustomers.Fields.CustomerFullName.Hint", "Customer full name"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.AffiliateCustomers.Fields.CustomerEmail", "Customer email"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.AffiliateCustomers.Fields.CustomerEmail.Hint", "Customer email"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.AffiliateCustomers.Fields.ApplyStatus", "Apply status"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.AffiliateCustomers.Fields.ApplyStatus.Hint", "Select affiliate apply status. By default 'Applied'"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.AffiliateCustomers.Fields.Active", "Active"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.AffiliateCustomers.Fields.Active.Hint", "Affiliate active status"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.AffiliateCustomers.Fields.OverrideCatalogCommission", "Override catalog commission"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.AffiliateCustomers.Fields.OverrideCatalogCommission.Hint", "By checking this, commission will be same for products for this affiliate account."),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.AffiliateCustomers.Fields.CommissionAmount", "Commission amount"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.AffiliateCustomers.Fields.CommissionAmount.Hint", "Enter fixed commission amount (per unit of product)"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.AffiliateCustomers.Fields.UsePercentage", "Use percentage"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.AffiliateCustomers.Fields.UsePercentage.Hint", "Use percentage"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.AffiliateCustomers.Fields.CommissionPercentage", "Commission percentage"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.AffiliateCustomers.Fields.CommissionPercentage.Hint", "Enter commission percentage"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.AffiliateCustomers.Fields.CreatedOn", "Created on"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.AffiliateCustomers.Fields.CreatedOn.Hint", "Created on"),

                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.AffiliateCustomers.List.AffiliateFirstName", "Affiliate first name"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.AffiliateCustomers.List.AffiliateFirstName.Hint", "Search by affiliate first name."),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.AffiliateCustomers.List.AffiliateLastName", "Affiliate last name"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.AffiliateCustomers.List.AffiliateLastName.Hint", "Search by affiliate last name."),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.AffiliateCustomers.List.CustomerEmail", "Customer email"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.AffiliateCustomers.List.CustomerFullName.Hint", "Search by customer email."),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.AffiliateCustomers.List.ApplyStatusIds", "Apply status"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.AffiliateCustomers.List.ApplyStatusIds.Hint", "Search by affiliate apply statuses."),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.AffiliateCustomers.List.CreatedFrom", "Created from"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.AffiliateCustomers.List.CreatedFrom.Hint", "The affiliate created from date."),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.AffiliateCustomers.List.CreatedTo", "Created to"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.AffiliateCustomers.List.CreatedTo.Hint", "The affiliate created to date."),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.AffiliateCustomers.List.ActiveStatusId", "Active"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.AffiliateCustomers.List.ActiveStatusId.Hint", "Search by affiliate active statuse."),

                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.CatalogCommissions.Fields.Name", "Name"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.CatalogCommissions.Fields.Name.Hint", "Name"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.CatalogCommissions.Fields.Entity", "Entity"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.CatalogCommissions.Fields.Entity.Hint", "Entity"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.CatalogCommissions.Fields.CommissionAmount", "Commission amount"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.CatalogCommissions.Fields.CommissionAmount.Hint", "Enter fixed commission amount (per unit of product)"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.CatalogCommissions.Fields.UsePercentage", "Use percentage"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.CatalogCommissions.Fields.UsePercentage.Hint", "Use percentage"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.CatalogCommissions.Fields.CommissionPercentage", "Commission percentage"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.CatalogCommissions.Fields.CommissionPercentage.Hint", "Enter commission percentage"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.CatalogCommissions.Fields.Commission", "Commission"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.CatalogCommissions.Fields.Commission.Hint", "Commission"),

                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.Configuration.Fields.UseDefaultCommissionIfNotSetOnCatalog", "Use default commission if not set on catalog"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.Configuration.Fields.UseDefaultCommissionIfNotSetOnCatalog.Hint", "By checking this, default commission will be calculated if it is not set on a catalog (Product/Category/Vendor)"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.Configuration.Fields.CommissionAmount", "Commission amount"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.Configuration.Fields.CommissionAmount.Hint", "Enter fixed commission amount (per unit of product)"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.Configuration.Fields.UsePercentage", "Use percentage"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.Configuration.Fields.UsePercentage.Hint", "Use percentage"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.Configuration.Fields.CommissionPercentage", "Commission percentage"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.Configuration.Fields.CommissionPercentage.Hint", "Enter commission percentage"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.Configuration.Fields.AffiliatePageOrderPageSize", "Affiliated order page size"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.Configuration.Fields.AffiliatePageOrderPageSize.Hint", "Page size for affiliated orders in customer's affiliate account."),

                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.OrderCommissions.Fields.OrderId", "Order #"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.OrderCommissions.Fields.OrderId.Hint", "Order number"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.OrderCommissions.Fields.TotalCommissionAmount", "Total commission"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.OrderCommissions.Fields.TotalCommissionAmount.Hint", "Total commission amount"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.OrderCommissions.Fields.CommissionStatus", "Commission status"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.OrderCommissions.Fields.CommissionStatus.Hint", "Commission status"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.OrderCommissions.Fields.CommissionPaidOn", "Commission paid on"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.OrderCommissions.Fields.CommissionPaidOn.Hint", "Commission paid on"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.OrderCommissions.Fields.OrderStatus", "Order status"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.OrderCommissions.Fields.OrderStatus.Hint", "Order status"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.OrderCommissions.Fields.PaymentStatus", "Payment status"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.OrderCommissions.Fields.PaymentStatus.Hint", "Payment status"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.OrderCommissions.Fields.PartialPaidAmount", "Partial paid amount"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.OrderCommissions.Fields.PartialPaidAmount.Hint", "Partial paid amount of Order commission."),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.OrderCommissions.Fields.Affiliate", "Affiliate"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.OrderCommissions.Fields.Affiliate.Hint", "Affiliate"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.OrderCommissions.Fields.Customer", "Customer"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.OrderCommissions.Fields.Customer.Hint", "Customer"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.OrderCommissions.Fields.CreatedOn", "Created on"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.OrderCommissions.Fields.CreatedOn.Hint", "Created on"),

                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.OrderCommissions.List.CommissionStatus", "Commission status"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.OrderCommissions.List.CommissionStatus.Hint", "Search by a commission payment status e.g. Paid."),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.OrderCommissions.List.OrderStatus", "Order status"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.OrderCommissions.List.OrderStatus.Hint", "Search by a specific order status e.g. Complete."),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.OrderCommissions.List.PaymentStatus", "Payment status"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.OrderCommissions.List.PaymentStatus.Hint", "Search by a specific payment status e.g. Paid."),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.OrderCommissions.List.AffiliateFirstName", "Affiliate first name"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.OrderCommissions.List.AffiliateFirstName.Hint", "Search by affiliate first name."),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.OrderCommissions.List.AffiliateLastName", "Affiliate last name"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.OrderCommissions.List.AffiliateLastName.Hint", "Search by affiliate last name."),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.OrderCommissions.List.StartDate", "Start date"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.OrderCommissions.List.StartDate.Hint", "The start date for the search."),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.OrderCommissions.List.EndDate", "End date"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.OrderCommissions.List.EndDate.Hint", "The end date for the search."),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.OrderCommissions.List", "Affiliated order commissions"),

                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.OrderCommissions.Fields.TotalCommissionAmount.Change", "Change"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.OrderCommissions.Fields.TotalCommissionAmount.Change.ForAdvancedUsers", "This option is only for advanced users (not recommended to change manually)."),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.OrderCommissions.Fields.CommissionStatus.Change.ForPaidOrders", "Commission status can be changed only for paid orders."),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.AffiliateCustomers.NotFound", "Affiliate customer not found."),

                new KeyValuePair<string, string>("NopStation.AffiliateStation.WebSite.AffiliateInfo", "Affiliate info"),
                new KeyValuePair<string, string>("NopStation.AffiliateStation.WebSite.ApplyForAffiliate", "Apply for affiliate"),
                new KeyValuePair<string, string>("NopStation.AffiliateStation.Account.AffiliateInfo", "Affiliate info"),
                new KeyValuePair<string, string>("NopStation.AffiliateStation.PageTitle.AffiliateInfo", "Affiliate info"),
                new KeyValuePair<string, string>("NopStation.AffiliateStation.WebSite.AffiliatedOrders", "Affiliated orders"),
                new KeyValuePair<string, string>("NopStation.AffiliateStation.PageTitle.AffiliatedOrders", "Affiliated orders"),
                new KeyValuePair<string, string>("NopStation.AffiliateStation.Account.AffiliatedOrders", "Affiliated orders"),
                new KeyValuePair<string, string>("NopStation.AffiliateStation.Account.ApplyForAffiliate", "Apply for affiliate"),
                new KeyValuePair<string, string>("NopStation.AffiliateStation.Account.AffiliateDetails", "Affiliate details"),
                new KeyValuePair<string, string>("NopStation.AffiliateStation.Account.AffiliateAddress", "Address details"),
                new KeyValuePair<string, string>("NopStation.AffiliateStation.Account.AffiliateContactInformation", "Contact information"),
                new KeyValuePair<string, string>("NopStation.AffiliateStation.Account.UpdateButton", "Update"),
                new KeyValuePair<string, string>("NopStation.AffiliateStation.Account.UpdateButton", "Update"),
                new KeyValuePair<string, string>("NopStation.AffiliateStation.Account.ApplyButton", "Apply"),
                new KeyValuePair<string, string>("NopStation.AffiliateStation.Account.CopyButton", "Copy"),
                new KeyValuePair<string, string>("NopStation.AffiliateStation.Account.UrlCopied", "Url copied successfully"),
                new KeyValuePair<string, string>("NopStation.AffiliateStation.Account.AppliedWarning", "Your affiliate application is not approved yet."),
                new KeyValuePair<string, string>("NopStation.AffiliateStation.Account.RejectedWarning", "Your affiliate application is rejected."),
                new KeyValuePair<string, string>("NopStation.AffiliateStation.Account.NotActiveWarning", "Your affiliate account is not active."),

                new KeyValuePair<string, string>("NopStation.AffiliateStation.Account.URL", "Url"),
                new KeyValuePair<string, string>("NopStation.AffiliateStation.Account.FriendlyUrlName", "Friendly url name"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.AffiliateCustomers.Active", "Active"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.AffiliateCustomers.InActive", "Inactive"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.AffiliateCustomers.List.ApplyStatusIds.All", "All"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.OrderCommissions.Report.Summary", "Summary"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.OrderCommissions.Report.Total", "Total"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.OrderCommissions.Report.Payable", "Payable"),
                new KeyValuePair<string, string>("Admin.NopStation.AffiliateStation.OrderCommissions.Report.Paid", "Paid"),

                new KeyValuePair<string, string>("NopStation.AffiliateStation.Account.CommissionSummarry", "Commission Summarry"),
                new KeyValuePair<string, string>("NopStation.AffiliateStation.Account.TotalCommission", "Total"),
                new KeyValuePair<string, string>("NopStation.AffiliateStation.Account.PaidCommission", "Paid"),
                new KeyValuePair<string, string>("NopStation.AffiliateStation.Account.PayableCommission", "Payable"),
                new KeyValuePair<string, string>("NopStation.AffiliateStation.Account.OrderList", "Order list"),
                new KeyValuePair<string, string>("NopStation.AffiliateStation.Account.OrderList.OrderId", "Order Id"),
                new KeyValuePair<string, string>("NopStation.AffiliateStation.Account.OrderList.TotalCommissionAmount", "Total Commission"),
                new KeyValuePair<string, string>("NopStation.AffiliateStation.Account.OrderList.CommissionStatus", "Commission Status"),
                new KeyValuePair<string, string>("NopStation.AffiliateStation.Account.OrderList.CommissionPaidOn", "Commission Paid On"),
                new KeyValuePair<string, string>("NopStation.AffiliateStation.Account.OrderList.OrderStatus", "Order Status"),
                new KeyValuePair<string, string>("NopStation.AffiliateStation.Account.OrderList.PaymentStatus", "Payment Status"),
                new KeyValuePair<string, string>("NopStation.AffiliateStation.Account.OrderList.PaidCommissionAmount", "Paid Commission Amount"),
                new KeyValuePair<string, string>("NopStation.AffiliateStation.Account.OrderList.CreatedOn", "Created On"),

                new KeyValuePair<string, string>("Enums.NopStation.Plugin.Widgets.AffiliateStation.Domains.ApplyStatus.Applied", "Applied"),
                new KeyValuePair<string, string>("Enums.NopStation.Plugin.Widgets.AffiliateStation.Domains.ApplyStatus.Approved", "Approved"),
                new KeyValuePair<string, string>("Enums.NopStation.Plugin.Widgets.AffiliateStation.Domains.ApplyStatus.Rejected", "Rejected"),
                new KeyValuePair<string, string>("Enums.NopStation.Plugin.Widgets.AffiliateStation.Domains.CommissionStatus.Pending", "Pending"),
                new KeyValuePair<string, string>("Enums.NopStation.Plugin.Widgets.AffiliateStation.Domains.CommissionStatus.PartiallyPaid", "Partially paid"),
                new KeyValuePair<string, string>("Enums.NopStation.Plugin.Widgets.AffiliateStation.Domains.CommissionStatus.Paid", "Paid")
            };

            return list;
        }

        #endregion
    }
}
