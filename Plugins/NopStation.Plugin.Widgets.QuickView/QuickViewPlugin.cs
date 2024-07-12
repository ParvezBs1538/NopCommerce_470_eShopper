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
using NopStation.Plugin.Widgets.QuickView.Components;

namespace NopStation.Plugin.Widgets.QuickView;

public class QuickViewPlugin : BasePlugin, IWidgetPlugin, IAdminMenuPlugin, INopStationPlugin
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

    public QuickViewPlugin(IWebHelper webHelper,
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
        return $"{_webHelper.GetStoreLocation()}Admin/QuickView/Configure";
    }

    public Type GetWidgetViewComponent(string widgetZone)
    {
        return typeof(QuickViewViewComponent);
    }

    public Task<IList<string>> GetWidgetZonesAsync()
    {
        return Task.FromResult<IList<string>>(new List<string> { PublicWidgetZones.Footer });
    }

    public async Task ManageSiteMapAsync(SiteMapNode rootNode)
    {
        var menuItem = new SiteMapNode()
        {
            Title = await _localizationService.GetResourceAsync("Admin.NopStation.QuickView.Menu.QuickView"),
            Visible = true,
            IconClass = "far fa-dot-circle",
        };

        if (await _permissionService.AuthorizeAsync(QuickViewPermissionProvider.ManageQuickView))
        {
            var configItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.QuickView.Menu.Configuration"),
                Url = "~/Admin/QuickView/Configure",
                Visible = true,
                IconClass = "far fa-circle",
                SystemName = "QuickView.Configuration"
            };
            menuItem.ChildNodes.Add(configItem);
        }

        var documentation = new SiteMapNode()
        {
            Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
            Url = "https://www.nop-station.com/quick-view-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=quick-view",
            Visible = true,
            IconClass = "far fa-circle",
            OpenUrlInNewTab = true
        };
        menuItem.ChildNodes.Add(documentation);

        await _nopStationCoreService.ManageSiteMapAsync(rootNode, menuItem, NopStationMenuType.Plugin);
    }

    public override async Task InstallAsync()
    {
        var quickViewSettings = new QuickViewSettings()
        {
            ShowAlsoPurchasedProducts = true,
            ShowRelatedProducts = true,
            ShowAvailability = false,
            ShowAddToWishlistButton = true,
            ShowProductEmailAFriendButton = false,
            EnablePictureZoom = true,
            ShowCompareProductsButton = false,
            ShowDeliveryInfo = false,
            ShowFullDescription = false,
            ShowProductManufacturers = false,
            ShowProductReviewOverview = false,
            ShowProductSpecifications = false,
            ShowProductTags = false,
            ShowShortDescription = false
        };
        await _settingService.SaveSettingAsync(quickViewSettings);

        await this.InstallPluginAsync(new QuickViewPermissionProvider());
        await base.InstallAsync();
    }

    public override async Task UninstallAsync()
    {
        await this.UninstallPluginAsync(new QuickViewPermissionProvider());
        await base.UninstallAsync();
    }

    public List<KeyValuePair<string, string>> PluginResouces()
    {
        var list = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("NopStation.QuickView.Button.QuickView", "Quick view"),
            new KeyValuePair<string, string>("NopStation.QuickView.Failed", "Failed to load quick view"),

            new KeyValuePair<string, string>("Admin.NopStation.QuickView.Menu.QuickView", "Quick view"),
            new KeyValuePair<string, string>("Admin.NopStation.QuickView.Menu.Configuration", "Configuration"),
            new KeyValuePair<string, string>("Admin.NopStation.QuickView.Configuration", "Quick view settings"),
            new KeyValuePair<string, string>("Admin.NopStation.QuickView.Configuration.Fields.ShowAlsoPurchasedProducts", "Show also purchased products"),
            new KeyValuePair<string, string>("Admin.NopStation.QuickView.Configuration.Fields.ShowAlsoPurchasedProducts.Hint", "Check to show \"Also purchased products\" on quick view page."),
            new KeyValuePair<string, string>("Admin.NopStation.QuickView.Configuration.Fields.ShowRelatedProducts", "Show related products"),
            new KeyValuePair<string, string>("Admin.NopStation.QuickView.Configuration.Fields.ShowRelatedProducts.Hint", "Check to show \"Related products\" on quick view page."),
            new KeyValuePair<string, string>("Admin.NopStation.QuickView.Configuration.Fields.EnablePictureZoom", "Enable picture zoom"),
            new KeyValuePair<string, string>("Admin.NopStation.QuickView.Configuration.Fields.EnablePictureZoom.Hint", "Check to enable picture zoom. Make sure Nop-Station picture zoom plugin is installed and activated for your store."),
            new KeyValuePair<string, string>("Admin.NopStation.QuickView.Configuration.Fields.ShowShortDescription", "Show short description"),
            new KeyValuePair<string, string>("Admin.NopStation.QuickView.Configuration.Fields.ShowShortDescription.Hint", "Check to show short description in quick view modal."),
            new KeyValuePair<string, string>("Admin.NopStation.QuickView.Configuration.Fields.ShowFullDescription", "Show full description"),
            new KeyValuePair<string, string>("Admin.NopStation.QuickView.Configuration.Fields.ShowFullDescription.Hint", "Check to show full description in quick view modal."),
            new KeyValuePair<string, string>("Admin.NopStation.QuickView.Configuration.Fields.ShowAddToWishlistButton", "Show add to wishlist button"),
            new KeyValuePair<string, string>("Admin.NopStation.QuickView.Configuration.Fields.ShowAddToWishlistButton.Hint", "Check to show 'Add To Wishlist' button in quick view modal."),
            new KeyValuePair<string, string>("Admin.NopStation.QuickView.Configuration.Fields.ShowCompareProductsButton", "Show compare products button"),
            new KeyValuePair<string, string>("Admin.NopStation.QuickView.Configuration.Fields.ShowCompareProductsButton.Hint", "Check to show 'Add to compare list' button in quick view modal."),
            new KeyValuePair<string, string>("Admin.NopStation.QuickView.Configuration.Fields.ShowProductEmailAFriendButton", "Show producte mail a friend button"),
            new KeyValuePair<string, string>("Admin.NopStation.QuickView.Configuration.Fields.ShowProductEmailAFriendButton.Hint", "Check to show 'Email a friend' button in quick view modal."),
            new KeyValuePair<string, string>("Admin.NopStation.QuickView.Configuration.Fields.ShowProductReviewOverview", "Show product review overview"),
            new KeyValuePair<string, string>("Admin.NopStation.QuickView.Configuration.Fields.ShowProductReviewOverview.Hint", "Check to show product review overview in quick view modal."),
            new KeyValuePair<string, string>("Admin.NopStation.QuickView.Configuration.Fields.ShowProductManufacturers", "Show product manufacturers"),
            new KeyValuePair<string, string>("Admin.NopStation.QuickView.Configuration.Fields.ShowProductManufacturers.Hint", "Check to show product manufacturers in quick view modal."),
            new KeyValuePair<string, string>("Admin.NopStation.QuickView.Configuration.Fields.ShowAvailability", "Show availability"),
            new KeyValuePair<string, string>("Admin.NopStation.QuickView.Configuration.Fields.ShowAvailability.Hint", "Check to show product availability in quick view modal."),
            new KeyValuePair<string, string>("Admin.NopStation.QuickView.Configuration.Fields.ShowDeliveryInfo", "Show delivery info"),
            new KeyValuePair<string, string>("Admin.NopStation.QuickView.Configuration.Fields.ShowDeliveryInfo.Hint", "Check to show product delivery information in quick view modal."),
            new KeyValuePair<string, string>("Admin.NopStation.QuickView.Configuration.Fields.ShowProductSpecifications", "Show product specifications"),
            new KeyValuePair<string, string>("Admin.NopStation.QuickView.Configuration.Fields.ShowProductSpecifications.Hint", "Check to show product specifications in quick view modal."),
            new KeyValuePair<string, string>("Admin.NopStation.QuickView.Configuration.Fields.ShowProductTags", "Show product tags"),
            new KeyValuePair<string, string>("Admin.NopStation.QuickView.Configuration.Fields.ShowProductTags.Hint", "Check to show product tags in quick view modal."),
            new KeyValuePair<string, string>("Admin.NopStation.QuickView.Configuration.Updated", "Quick view configuration updated successfully.")
        };

        return list;
    }

    #endregion
}
