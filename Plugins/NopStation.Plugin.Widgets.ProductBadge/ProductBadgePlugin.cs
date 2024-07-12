using System;
using System.Collections.Generic;
using System.Linq;
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
using NopStation.Plugin.Widgets.ProductBadge.Components;

namespace NopStation.Plugin.Widgets.ProductBadge;

public class ProductBadgePlugin : BasePlugin, IWidgetPlugin, IAdminMenuPlugin, INopStationPlugin
{
    #region Fields

    private readonly IWebHelper _webHelper;
    private readonly IPermissionService _permissionService;
    private readonly INopStationCoreService _nopStationCoreService;
    private readonly ILocalizationService _localizationService;
    private readonly ProductBadgeSettings _productBadgeSettings;
    private readonly ISettingService _settingService;

    public bool HideInWidgetList => false;

    #endregion Fields

    #region Ctor

    public ProductBadgePlugin(IWebHelper webHelper,
        IPermissionService permissionService,
        INopStationCoreService nopStationCoreService,
        ILocalizationService localizationService,
        ProductBadgeSettings productBadgeSettings,
        ISettingService settingService)
    {
        _webHelper = webHelper;
        _permissionService = permissionService;
        _nopStationCoreService = nopStationCoreService;
        _localizationService = localizationService;
        _productBadgeSettings = productBadgeSettings;
        _settingService = settingService;
    }

    #endregion

    #region Methods

    public override string GetConfigurationPageUrl()
    {
        return $"{_webHelper.GetStoreLocation()}Admin/ProductBadge/Configure";
    }

    public Type GetWidgetViewComponent(string widgetZone)
    {
        if (widgetZone == PublicWidgetZones.Footer)
            return typeof(ProductBadgeFooterViewComponent);

        return typeof(ProductBadgeViewComponent);
    }

    public Task<IList<string>> GetWidgetZonesAsync()
    {
        var detailsPageWidgetZone = string.IsNullOrWhiteSpace(_productBadgeSettings.ProductDetailsWidgetZone) ?
            PublicWidgetZones.ProductDetailsBeforePictures : _productBadgeSettings.ProductDetailsWidgetZone;

        var overviewWidgetZone = string.IsNullOrWhiteSpace(_productBadgeSettings.ProductBoxWidgetZone) ?
            PublicWidgetZones.ProductBoxAddinfoBefore : _productBadgeSettings.ProductBoxWidgetZone;

        return Task.FromResult<IList<string>>(new List<string>
        {
            detailsPageWidgetZone,
            overviewWidgetZone,
            PublicWidgetZones.Footer
        });
    }

    public async Task ManageSiteMapAsync(SiteMapNode rootNode)
    {
        if (await _permissionService.AuthorizeAsync(ProductBadgePermissionProvider.ManageProductBadge))
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.ProductBadge.Menu.ProductBadge"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            var configItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.ProductBadge.Menu.Configuration"),
                Url = "~/Admin/ProductBadge/Configure",
                Visible = true,
                IconClass = "far fa-circle",
                SystemName = "ProductBadge.Configuration"
            };
            menuItem.ChildNodes.Add(configItem);

            var listItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.ProductBadge.Menu.ProductBadges"),
                Url = "~/Admin/ProductBadge/List",
                Visible = true,
                IconClass = "far fa-circle",
                SystemName = "Product badges"
            };
            menuItem.ChildNodes.Add(listItem);

            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ManageConfiguration))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/smart-product-badge-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=smart-product-badge",
                    Visible = true,
                    IconClass = "far fa-circle",
                    OpenUrlInNewTab = true
                };
                menuItem.ChildNodes.Add(documentation);
            }

            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menuItem, NopStationMenuType.Plugin);
        }
    }

    public override async Task InstallAsync()
    {
        await _settingService.SaveSettingAsync(new ProductBadgeSettings
        {
            ProductDetailsWidgetZone = PublicWidgetZones.ProductDetailsBeforePictures,
            ProductBoxWidgetZone = PublicWidgetZones.ProductBoxAddinfoBefore,
            EnableAjaxLoad = false,
            CacheActiveBadges = true,
            IncreaseWidthInDetailsPageByPercentage = 10,
            LargeBadgeWidth = 90,
            MediumBadgeWidth = 80,
            SmallBadgeWidth = 70
        });

        await this.InstallPluginAsync(new ProductBadgePermissionProvider());
        await base.InstallAsync();
    }

    public override async Task UninstallAsync()
    {
        await this.UninstallPluginAsync(new ProductBadgePermissionProvider());
        await base.UninstallAsync();
    }

    public List<KeyValuePair<string, string>> PluginResouces()
    {
        var list = new Dictionary<string, string>
        {
            ["Admin.NopStation.ProductBadge.Menu.ProductBadge"] = "Product badge",
            ["Admin.NopStation.ProductBadge.Menu.Configuration"] = "Configuration",
            ["Admin.NopStation.ProductBadge.Menu.ProductBadges"] = "Badges",

            ["Admin.NopStation.ProductBadge.Configuration.Fields.EnableAjaxLoad"] = "Enable AJAX load",
            ["Admin.NopStation.ProductBadge.Configuration.Fields.EnableAjaxLoad.Hint"] = "Determines whether badges will load on AJAX.",
            ["Admin.NopStation.ProductBadge.Configuration.Fields.CacheActiveBadges"] = "Cache active badges",
            ["Admin.NopStation.ProductBadge.Configuration.Fields.CacheActiveBadges.Hint"] = "Check to cache active badges.",
            ["Admin.NopStation.ProductBadge.Configuration.Fields.ProductDetailsWidgetZone"] = "Product details widget zone",
            ["Admin.NopStation.ProductBadge.Configuration.Fields.ProductDetailsWidgetZone.Hint"] = "Specify the widget zone where the badge will be appeared in product details page. (i.e. productdetails_before_pictures)",
            ["Admin.NopStation.ProductBadge.Configuration.Fields.ProductBoxWidgetZone"] = "Product box widget zone",
            ["Admin.NopStation.ProductBadge.Configuration.Fields.ProductBoxWidgetZone.Hint"] = "Specify the widget zone where the badge will be appeared in product overview box. (i.e. productbox_addinfo_before)",
            ["Admin.NopStation.ProductBadge.Configuration.Fields.SmallBadgeWidth"] = "Small badge width",
            ["Admin.NopStation.ProductBadge.Configuration.Fields.SmallBadgeWidth.Hint"] = "The width of small badge in pixel.",
            ["Admin.NopStation.ProductBadge.Configuration.Fields.MediumBadgeWidth"] = "Medium badge width",
            ["Admin.NopStation.ProductBadge.Configuration.Fields.MediumBadgeWidth.Hint"] = "The width of medium badge in pixel.",
            ["Admin.NopStation.ProductBadge.Configuration.Fields.LargeBadgeWidth"] = "Large badge width",
            ["Admin.NopStation.ProductBadge.Configuration.Fields.LargeBadgeWidth.Hint"] = "The width of large badge in pixel.",
            ["Admin.NopStation.ProductBadge.Configuration.Fields.IncreaseWidthInDetailsPageByPercentage"] = "Increase width in details page by percentage",
            ["Admin.NopStation.ProductBadge.Configuration.Fields.IncreaseWidthInDetailsPageByPercentage.Hint"] = "Increase width in details page by percentage.",

            ["Admin.NopStation.ProductBadge.Badges.BestSell.Parameters"] = "Best selleing parameters",
            ["Admin.NopStation.ProductBadge.Badges.List"] = "Product badges",
            ["Admin.NopStation.ProductBadge.Badges.List.SearchActive.Active"] = "Active",
            ["Admin.NopStation.ProductBadge.Badges.List.SearchActive.Inactive"] = "Inactive",
            ["Admin.NopStation.ProductBadge.Badges.List.SearchStore"] = "Store",
            ["Admin.NopStation.ProductBadge.Badges.List.SearchStore.Hint"] = "The search store.",
            ["Admin.NopStation.ProductBadge.Badges.List.SearchActive"] = "Active",
            ["Admin.NopStation.ProductBadge.Badges.List.SearchActive.Hint"] = "The search active.",
            ["Admin.NopStation.ProductBadge.Badges.List.SearchKeyword"] = "Keyword",
            ["Admin.NopStation.ProductBadge.Badges.List.SearchKeyword.Hint"] = "The search keyword.",

            ["Admin.NopStation.ProductBadge.Badges.Updated"] = "Badge has been updated successfully.",
            ["Admin.NopStation.ProductBadge.Badges.Created"] = "Badge has been created successfully.",
            ["Admin.NopStation.ProductBadge.Badges.Deleted"] = "Badge has been deleted successfully.",

            ["Admin.NopStation.ProductBadge.Badges.Fields.BestSellSoldInDays"] = "Sold in days",
            ["Admin.NopStation.ProductBadge.Badges.Fields.BestSellSoldInDays.Hint"] = "Sold in days (i.e. 10, 30).",
            ["Admin.NopStation.ProductBadge.Badges.Fields.BestSellStoreWise"] = "Store-wise",
            ["Admin.NopStation.ProductBadge.Badges.Fields.BestSellStoreWise.Hint"] = "Check to calculate best selling product per store.",
            ["Admin.NopStation.ProductBadge.Badges.Fields.BestSellMinimumAmountSold"] = "Minimum amount sold",
            ["Admin.NopStation.ProductBadge.Badges.Fields.BestSellMinimumAmountSold.Hint"] = "Enter minimum amount of sell to be marked as best seller.",
            ["Admin.NopStation.ProductBadge.Badges.Fields.BestSellMinimumQuantitySold"] = "Minimum quantity sold",
            ["Admin.NopStation.ProductBadge.Badges.Fields.BestSellMinimumQuantitySold.Hint"] = "Enter minimum quantity of sell to be marked as best seller.",
            ["Admin.NopStation.ProductBadge.Badges.Fields.BestSellPaymentStatusIds"] = "Payment statuses",
            ["Admin.NopStation.ProductBadge.Badges.Fields.BestSellPaymentStatusIds.Hint"] = "Select best sell payment status options.",
            ["Admin.NopStation.ProductBadge.Badges.Fields.BestSellOrderStatusIds"] = "Order statuses",
            ["Admin.NopStation.ProductBadge.Badges.Fields.BestSellOrderStatusIds.Hint"] = "Select best sell order status options.",
            ["Admin.NopStation.ProductBadge.Badges.Fields.BestSellShippingStatusIds"] = "Shipping statuses",
            ["Admin.NopStation.ProductBadge.Badges.Fields.BestSellShippingStatusIds.Hint"] = "Select best sell shipping status options.",

            ["Admin.NopStation.ProductBadge.Badges.Fields.Name"] = "Name",
            ["Admin.NopStation.ProductBadge.Badges.Fields.Name.Hint"] = "The badge name.",
            ["Admin.NopStation.ProductBadge.Badges.Fields.ContentType"] = "Content type",
            ["Admin.NopStation.ProductBadge.Badges.Fields.ContentType.Hint"] = "Specify the badge content type.",
            ["Admin.NopStation.ProductBadge.Badges.Fields.ShapeType"] = "Shape",
            ["Admin.NopStation.ProductBadge.Badges.Fields.ShapeType.Hint"] = "Specify the badge shape.",
            ["Admin.NopStation.ProductBadge.Badges.Fields.Size"] = "Size",
            ["Admin.NopStation.ProductBadge.Badges.Fields.Size.Hint"] = "Specify the size of badge in product overview box.",
            ["Admin.NopStation.ProductBadge.Badges.Fields.DiscountBadgeTextFormat"] = "Text format",
            ["Admin.NopStation.ProductBadge.Badges.Fields.DiscountBadgeTextFormat.Hint"] = "Specify the badge text format.",
            ["Admin.NopStation.ProductBadge.Badges.Fields.BadgeType"] = "Badge type",
            ["Admin.NopStation.ProductBadge.Badges.Fields.BadgeType.Hint"] = "Specify the badge type.",
            ["Admin.NopStation.ProductBadge.Badges.Fields.BackgroundColor"] = "Background color",
            ["Admin.NopStation.ProductBadge.Badges.Fields.BackgroundColor.Hint"] = "Specify the badge background color.",
            ["Admin.NopStation.ProductBadge.Badges.Fields.FontColor"] = "Font color",
            ["Admin.NopStation.ProductBadge.Badges.Fields.FontColor.Hint"] = "Specify the badge font color.",
            ["Admin.NopStation.ProductBadge.Badges.Fields.Text"] = "Text",
            ["Admin.NopStation.ProductBadge.Badges.Fields.Text.Hint"] = "Specify the badge text.",
            ["Admin.NopStation.ProductBadge.Badges.Fields.Active"] = "Active",
            ["Admin.NopStation.ProductBadge.Badges.Fields.Active.Hint"] = "Determines whether this badge is active (visible on public store).",
            ["Admin.NopStation.ProductBadge.Badges.Fields.CssClass"] = "CSS class",
            ["Admin.NopStation.ProductBadge.Badges.Fields.CssClass.Hint"] = "Custom CSS class for the product badge.",
            ["Admin.NopStation.ProductBadge.Badges.Fields.Picture"] = "Picture",
            ["Admin.NopStation.ProductBadge.Badges.Fields.Picture.Hint"] = "The badge picture",
            ["Admin.NopStation.ProductBadge.Badges.Fields.PositionType"] = "Position",
            ["Admin.NopStation.ProductBadge.Badges.Fields.PositionType.Hint"] = "The default position where badge will appear.",
            ["Admin.NopStation.ProductBadge.Badges.Fields.PositionType.RtlHint"] = "The position for Left-to-Right (LTR) languages (i.e. English). For Right-to-Left (RTL) languages (i.e. Arabic) position will be opposite.",
            ["Admin.NopStation.ProductBadge.Badges.Fields.CatalogType"] = "Catalog type",
            ["Admin.NopStation.ProductBadge.Badges.Fields.CatalogType.Hint"] = "Specify the badge catalog type. For example, select 'Products' to apply this badge on specific products.",
            ["Admin.NopStation.ProductBadge.Badges.Fields.LimitedToStores"] = "Limited to stores",
            ["Admin.NopStation.ProductBadge.Badges.Fields.LimitedToStores.Hint"] = "Option to limit this badge to a certain store. If you have multiple stores, choose one or several from the list. If you don't use this option just leave this field empty.",
            ["Admin.NopStation.ProductBadge.Badges.Fields.AclCustomerRoles"] = "Customer roles",
            ["Admin.NopStation.ProductBadge.Badges.Fields.AclCustomerRoles.Hint"] = "Choose one or several customer roles i.e. administrators, vendors, guests, who will be able to see this product in catalog. If you don't need this option just leave this field empty. In order to use this functionality, you have to disable the following setting: Configuration &gt; Settings &gt; Catalog &gt; Ignore ACL rules (sitewide).",
            ["Admin.NopStation.ProductBadge.Badges.Fields.FromDateTimeUtc"] = "From date time UTC",
            ["Admin.NopStation.ProductBadge.Badges.Fields.FromDateTimeUtc.Hint"] = "Set the date time (UTC) from which this product badge will be displayed.",
            ["Admin.NopStation.ProductBadge.Badges.Fields.ToDateTimeUtc"] = "To date time UTC",
            ["Admin.NopStation.ProductBadge.Badges.Fields.ToDateTimeUtc.Hint"] = "Set the date time (UTC) till which this product badge will be displayed.",

            ["Admin.NopStation.ProductBadge.Badges.Fields.Name.Required"] = "Badge name is required.",
            ["Admin.NopStation.ProductBadge.Badges.Fields.Text.Required"] = "Badge text is required.",
            ["Admin.NopStation.ProductBadge.Badges.AddNew"] = "Add new badge",
            ["Admin.NopStation.ProductBadge.Badges.BackToList"] = "back to badge list",
            ["Admin.NopStation.ProductBadge.Badges.EditDetails"] = "Edit badge details",

            ["Admin.NopStation.ProductBadge.Badges.Tab.Info"] = "Info",
            ["Admin.NopStation.ProductBadge.Badges.Tab.AppliedToCategories"] = "Applied to categories",
            ["Admin.NopStation.ProductBadge.Badges.Tab.AppliedToManufacturers"] = "Applied to manufacturers",
            ["Admin.NopStation.ProductBadge.Badges.Tab.AppliedToVendors"] = "Applied to vendors",
            ["Admin.NopStation.ProductBadge.Badges.Tab.AppliedToProducts"] = "Applied to products",

            ["Admin.NopStation.ProductBadge.Badges.Categories.AddNew"] = "Add new category",
            ["Admin.NopStation.ProductBadge.Badges.Categories.SaveBeforeEdit"] = "You need to save the badge before you can add categories for this badge page.",
            ["Admin.NopStation.ProductBadge.Badges.Categories.Category"] = "Category",
            ["Admin.NopStation.ProductBadge.Badges.Categories.Published"] = "Published",

            ["Admin.NopStation.ProductBadge.Badges.Manufacturers.AddNew"] = "Add new manufacturer",
            ["Admin.NopStation.ProductBadge.Badges.Manufacturers.SaveBeforeEdit"] = "You need to save the badge before you can add manufacturers for this badge page.",
            ["Admin.NopStation.ProductBadge.Badges.Manufacturers.Manufacturer"] = "Manufacturer",
            ["Admin.NopStation.ProductBadge.Badges.Manufacturers.Published"] = "Published",

            ["Admin.NopStation.ProductBadge.Badges.Vendors.AddNew"] = "Add new vendor",
            ["Admin.NopStation.ProductBadge.Badges.Vendors.SaveBeforeEdit"] = "You need to save the badge before you can add vendors for this badge page.",
            ["Admin.NopStation.ProductBadge.Badges.Vendors.Vendor"] = "Vendor",
            ["Admin.NopStation.ProductBadge.Badges.Vendors.Active"] = "Active",

            ["Admin.NopStation.ProductBadge.Badges.Products.AddNew"] = "Add new product",
            ["Admin.NopStation.ProductBadge.Badges.Products.SaveBeforeEdit"] = "You need to save the badge before you can add products for this badge page.",
            ["Admin.NopStation.ProductBadge.Badges.Products.Product"] = "Product",
            ["Admin.NopStation.ProductBadge.Badges.Products.Published"] = "Published",

            ["NopStation.ProductBadge.LoadingFailed"] = "Failed to load badge content.",
        };

        return list.ToList();
    }

    #endregion
}