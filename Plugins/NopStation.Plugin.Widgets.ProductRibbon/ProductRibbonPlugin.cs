using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Widgets.ProductRibbon.Components;

namespace NopStation.Plugin.Widgets.ProductRibbon
{
    public class ProductRibbonPlugin : BasePlugin, IWidgetPlugin, IAdminMenuPlugin, INopStationPlugin
    {
        #region Fields

        public bool HideInWidgetList => false;

        private readonly IWebHelper _webHelper;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly ProductRibbonSettings _productRibbonSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;

        #endregion

        #region Ctor

        public ProductRibbonPlugin(IWebHelper webHelper,
            INopStationCoreService nopStationCoreService,
            ProductRibbonSettings productRibbonSettings,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            ISettingService settingService)
        {
            _webHelper = webHelper;
            _nopStationCoreService = nopStationCoreService;
            _productRibbonSettings = productRibbonSettings;
            _localizationService = localizationService;
            _permissionService = permissionService;
            _settingService = settingService;
        }

        #endregion

        #region Methods

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/ProductRibbon/Configure";
        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            if (widgetZone == PublicWidgetZones.Footer)
                return typeof(ProductRibbonFooterViewComponent);

            return typeof(ProductRibbonViewComponent);
        }

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            var detailsPageWidgetZone = string.IsNullOrWhiteSpace(_productRibbonSettings.ProductDetailsPageWidgetZone) ?
                PublicWidgetZones.ProductDetailsBeforePictures : _productRibbonSettings.ProductDetailsPageWidgetZone;

            var overviewWidgetZone = string.IsNullOrWhiteSpace(_productRibbonSettings.ProductOverviewBoxWidgetZone) ?
                PublicWidgetZones.ProductBoxAddinfoBefore : _productRibbonSettings.ProductOverviewBoxWidgetZone;
            return Task.FromResult<IList<string>>(new List<string>
            {
                detailsPageWidgetZone,
                overviewWidgetZone,
                PublicWidgetZones.Footer
            });
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.ProductRibbon.Menu.ProductRibbon"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(ProductRibbonPermissionProvider.ManageProductRibbon))
            {
                var configItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.ProductRibbon.Menu.Configuration"),
                    Url = $"{_webHelper.GetStoreLocation()}Admin/ProductRibbon/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "ProductRibbon.Configuration"
                };
                menuItem.ChildNodes.Add(configItem);
            }

            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/product-ribbon-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=product-ribbon",
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
            var settings = new ProductRibbonSettings()
            {
                EnableBestSellerRibbon = true,
                EnableDiscountRibbon = true,
                EnableNewRibbon = true,
                ProductDetailsPageWidgetZone = PublicWidgetZones.ProductDetailsBeforePictures,
                ProductOverviewBoxWidgetZone = PublicWidgetZones.ProductBoxAddinfoBefore,
                BestSellStoreWise = true,
                SoldInDays = 30,
                BestSellOrderStatusIds = new List<int>() { (int)OrderStatus.Complete, (int)OrderStatus.Complete },
                BestSellPaymentStatusIds = new List<int>() { (int)PaymentStatus.Paid },
                BestSellShippingStatusIds = new List<int>() { (int)ShippingStatus.Delivered, (int)ShippingStatus.Shipped, (int)ShippingStatus.ShippingNotRequired }
            };
            await _settingService.SaveSettingAsync(settings);

            await this.InstallPluginAsync(new ProductRibbonPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await this.UninstallPluginAsync(new ProductRibbonPermissionProvider());
            await base.UninstallAsync();
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Admin.NopStation.ProductRibbon.Menu.ProductRibbon", "Product ribbon"),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRibbon.Menu.Configuration", "Configuration"),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRibbon.Configuration", "Product ribbon settings"),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRibbon.Configuration.Fields.EnableNewRibbon", "Enable 'New' ribbon"),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRibbon.Configuration.Fields.EnableDiscountRibbon", "Enable 'Discount' ribbon"),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRibbon.Configuration.Fields.EnableBestSellerRibbon", "Enable 'Best Seller' ribbon"),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRibbon.Configuration.Fields.EnableNewRibbon.Hint", "Check to enable 'New' ribbon on product view (product overview box and details page)."),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRibbon.Configuration.Fields.EnableDiscountRibbon.Hint", "Check to enable 'Discount' ribbon on product view (product overview box and details page)"),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRibbon.Configuration.Fields.EnableBestSellerRibbon.Hint", "Check to enable 'Best Seller' ribbon on product view (product overview box and details page)"),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRibbon.Configuration.Fields.ProductDetailsPageWidgetZone.Required", "Product details page widget zone is required."),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRibbon.Configuration.Fields.ProductOverviewBoxWidgetZone.Required", "Product overview box widget zone is required."),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRibbon.Configuration.Fields.ProductDetailsPageWidgetZone", "Product details page widget zone"),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRibbon.Configuration.Fields.ProductDetailsPageWidgetZone.Hint", "Specify the widget zone where the ribbon will be appeared in product details page. (i.e. productdetails_before_pictures)"),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRibbon.Configuration.Fields.ProductOverviewBoxWidgetZone", "Product overview box widget zone"),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRibbon.Configuration.Fields.ProductOverviewBoxWidgetZone.Hint", "Specify the widget zone where the ribbon will be appeared in product overview box. (i.e. productbox_addinfo_before)"),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRibbon.Configuration.Fields.SoldInDays", "Sold in days"),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRibbon.Configuration.Fields.SoldInDays.Hint", "Sold in days (i.e. 10, 30)."),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRibbon.Configuration.Fields.BestSellStoreWise", "Best sell store-wise"),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRibbon.Configuration.Fields.BestSellStoreWise.Hint", "Check to calculate best selling product per store."),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRibbon.Configuration.Fields.BestSellPaymentStatus", "Best sell payment status"),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRibbon.Configuration.Fields.BestSellPaymentStatus.Hint", "Select best sell payment status options."),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRibbon.Configuration.Fields.BestSellOrderStatus", "Best sell order status"),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRibbon.Configuration.Fields.BestSellOrderStatus.Hint", "Select best sell order status options."),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRibbon.Configuration.Fields.BestSellShippingStatus", "Best sell shipping status"),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRibbon.Configuration.Fields.BestSellShippingStatus.Hint", "Select best sell shipping status options."),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRibbon.Configuration.Fields.MinimumAmountSold", "Minimum amount sold"),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRibbon.Configuration.Fields.MinimumAmountSold.Hint", "Enter minimum amount of sell to be marked as best seller."),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRibbon.Configuration.Fields.MinimumQuantitySold", "Minimum quantity sold"),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRibbon.Configuration.Fields.MinimumQuantitySold.Hint", "Enter minimum quantity of sell to be marked as best seller."),
                new KeyValuePair<string, string>("Admin.NopStation.ProductRibbon.Configuration.Updated", "Product ribbon settings updated successfully."),

                new KeyValuePair<string, string>("NopStation.ProductRibbon.RibbonText.New", "New"),
                new KeyValuePair<string, string>("NopStation.ProductRibbon.RibbonText.Discount", "{0}% Off"),
                new KeyValuePair<string, string>("NopStation.ProductRibbon.RibbonText.BestSeller", "Best Seller")
            };

            return list;
        }

        #endregion
    }
}
