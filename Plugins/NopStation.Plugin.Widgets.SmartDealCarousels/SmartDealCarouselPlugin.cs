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
using NopStation.Plugin.Misc.WidgetManager.Services;
using NopStation.Plugin.Widgets.SmartDealCarousels.Components;
using NopStation.Plugin.Widgets.SmartDealCarousels.Domains;

namespace NopStation.Plugin.Widgets.SmartDealCarousels;

public class SmartDealCarouselPlugin : BasePlugin, IWidgetPlugin, IAdminMenuPlugin, INopStationPlugin
{
    #region Fields

    public bool HideInWidgetList => false;

    private readonly IWebHelper _webHelper;
    private readonly INopStationCoreService _nopStationCoreService;
    private readonly ILocalizationService _localizationService;
    private readonly IPermissionService _permissionService;
    private readonly ISettingService _settingService;
    private readonly IWidgetZoneService _widgetZoneService;

    #endregion

    #region Ctor

    public SmartDealCarouselPlugin(IWebHelper webHelper,
        INopStationCoreService nopStationCoreService,
        ILocalizationService localizationService,
        IPermissionService permissionService,
        ISettingService settingService,
        IWidgetZoneService widgetZoneService)
    {
        _webHelper = webHelper;
        _nopStationCoreService = nopStationCoreService;
        _localizationService = localizationService;
        _permissionService = permissionService;
        _settingService = settingService;
        _widgetZoneService = widgetZoneService;
    }

    #endregion

    #region Methods

    public override string GetConfigurationPageUrl()
    {
        return $"{_webHelper.GetStoreLocation()}Admin/SmartDealCarousel/Configure";
    }

    public Type GetWidgetViewComponent(string widgetZone)
    {
        if (widgetZone == PublicWidgetZones.Footer)
            return typeof(SmartDealCarouselFooterViewComponent);

        return typeof(SmartDealCarouselViewComponent);
    }

    public async Task<IList<string>> GetWidgetZonesAsync()
    {
        var zones = (await _widgetZoneService.GetWidgetZonesForDomainAsync<SmartDealCarousel>()).ToList();
        if (!zones.Contains(PublicWidgetZones.Footer))
            zones.Add(PublicWidgetZones.Footer);

        return zones;
    }

    public async Task ManageSiteMapAsync(SiteMapNode rootNode)
    {
        if (await _permissionService.AuthorizeAsync(SmartDealCarouselPermissionProvider.ManageSmartDealCarousels))
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.SmartDealCarousels.Menu.SmartDealCarousel"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            var listItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.SmartDealCarousels.Menu.Carousels"),
                Url = "~/Admin/SmartDealCarousel/List",
                Visible = true,
                IconClass = "far fa-circle",
                SystemName = "SmartDealCarousels"
            };
            menuItem.ChildNodes.Add(listItem);

            var configItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.SmartDealCarousels.Menu.Configuration"),
                Url = "~/Admin/SmartDealCarousel/Configure",
                Visible = true,
                IconClass = "far fa-circle",
                SystemName = "SmartDealCarousels.Configuration"
            };
            menuItem.ChildNodes.Add(configItem);

            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/smart-carousel-documentation?utm_source=admin-panel?utm_source=admin-panel&utm_medium=products&utm_campaign=smart-carousel",
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
        await _settingService.SaveSettingAsync(new SmartDealCarouselSettings
        {
            EnableAjaxLoad = true,
            EnableCarousel = true,
            CarouselPictureSize = 250
        });

        await this.InstallPluginAsync(new SmartDealCarouselPermissionProvider());
        await base.InstallAsync();
    }

    public override async Task UninstallAsync()
    {
        await this.UninstallPluginAsync(new SmartDealCarouselPermissionProvider());
        await base.UninstallAsync();
    }

    public List<KeyValuePair<string, string>> PluginResouces()
    {
        var list = new Dictionary<string, string>
        {
            ["Admin.NopStation.SmartDealCarousels.Menu.SmartDealCarousel"] = "Deal carousel",
            ["Admin.NopStation.SmartDealCarousels.Menu.Configuration"] = "Configuration",
            ["Admin.NopStation.SmartDealCarousels.Menu.Carousels"] = "Carousels",

            ["Admin.NopStation.SmartDealCarousels.Configuration"] = "Carousel settings",
            ["Admin.NopStation.SmartDealCarousels.Configuration.Fields.EnableCarousel"] = "Enable carousel",
            ["Admin.NopStation.SmartDealCarousels.Configuration.Fields.EnableCarousel.Hint"] = "Check to enable smart carousel for your store.",
            ["Admin.NopStation.SmartDealCarousels.Configuration.Fields.EnableAjaxLoad"] = "Enable AJAX load",
            ["Admin.NopStation.SmartDealCarousels.Configuration.Fields.EnableAjaxLoad.Hint"] = "Determines whether carousels will load on AJAX.",
            ["Admin.NopStation.SmartDealCarousels.Configuration.Fields.CarouselPictureSize"] = "Carousel picture size",
            ["Admin.NopStation.SmartDealCarousels.Configuration.Fields.CarouselPictureSize.Hint"] = "Thhe carousel picture size.",

            ["Admin.NopStation.SmartDealCarousels.Carousels.List"] = "Carousels",
            ["Admin.NopStation.SmartDealCarousels.Carousels.List.SearchActive.Active"] = "Active",
            ["Admin.NopStation.SmartDealCarousels.Carousels.List.SearchActive.Inactive"] = "Inactive",

            ["Admin.NopStation.SmartDealCarousels.Carousels.List.SearchKeyword"] = "Keyword",
            ["Admin.NopStation.SmartDealCarousels.Carousels.List.SearchKeyword.Hint"] = "The search keyword.",
            ["Admin.NopStation.SmartDealCarousels.Carousels.List.SearchWidgetZone"] = "Widget zone",
            ["Admin.NopStation.SmartDealCarousels.Carousels.List.SearchWidgetZone.Hint"] = "The search widget zone.",
            ["Admin.NopStation.SmartDealCarousels.Carousels.List.SearchProductSource"] = "Product source",
            ["Admin.NopStation.SmartDealCarousels.Carousels.List.SearchProductSource.Hint"] = "The search product source.",
            ["Admin.NopStation.SmartDealCarousels.Carousels.List.SearchCarouselType"] = "Carousel type",
            ["Admin.NopStation.SmartDealCarousels.Carousels.List.SearchCarouselType.Hint"] = "The search carousel type.",
            ["Admin.NopStation.SmartDealCarousels.Carousels.List.SearchStore"] = "Store",
            ["Admin.NopStation.SmartDealCarousels.Carousels.List.SearchStore.Hint"] = "The search store.",
            ["Admin.NopStation.SmartDealCarousels.Carousels.List.SearchActive"] = "Active",
            ["Admin.NopStation.SmartDealCarousels.Carousels.List.SearchActive.Hint"] = "The search active.",

            ["Admin.NopStation.SmartDealCarousels.Carousels.Tab.Info"] = "Info",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Tab.Properties"] = "Properties",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Tab.Products"] = "Carousel products",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Tab.Categories"] = "Carousel categories",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Tab.Manufacturers"] = "Carousel manufacturers",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Tab.Vendors"] = "Carousel vendors",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Tab.Pictures"] = "Carousel pictures",

            ["Admin.NopStation.SmartDealCarousels.Carousels.EditDetails"] = "Edit carousel details",
            ["Admin.NopStation.SmartDealCarousels.Carousels.BackToList"] = "back to carousel list",
            ["Admin.NopStation.SmartDealCarousels.Carousels.AddNew"] = "Add new carousel",

            ["Admin.NopStation.SmartDealCarousels.Carousels.Created"] = "Carousel has been created successfully.",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Updated"] = "Carousel has been updated successfully.",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Deleted"] = "Carousel has been deleted successfully.",

            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.Name"] = "Name",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.Name.Hint"] = "The carousel name.",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.Title"] = "Title",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.Title.Hint"] = "The carousel title.",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.DisplayTitle"] = "Display title",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.DisplayTitle.Hint"] = "Determines whether title should be displayed on public site (depends on theme design).",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.Active"] = "Active",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.Active.Hint"] = "Determines whether this carousel is active (visible on public store).",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.ProductSourceType"] = "Product source",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.ProductSourceType.Hint"] = "The product source for this carousel.",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.Discount"] = "Discount",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.Discount.Hint"] = "The discount for this carousel.",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.ShowCarouselPicture"] = "Show carousel picture",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.ShowCarouselPicture.Hint"] = "Determines whether to show carousel picture.",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.Picture"] = "Picture",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.Picture.Hint"] = "The carousel picture.",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.PicturePosition"] = "Picture position",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.PicturePosition.Hint"] = "The carousel picture position.",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.PicturePosition.RtlHint"] = "The position for Left-to-Right (LTR) languages (i.e. English). For Right-to-Left (RTL) languages (i.e. Arabic) position will be opposite.",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.ShowBackground"] = "Show background",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.ShowBackground.Hint"] = "Determines whether to show background.",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.BackgroundType"] = "Background type",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.BackgroundType.Hint"] = "The carousel background type.",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.BackgroundColor"] = "Background color",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.BackgroundColor.Hint"] = "The carousel background color.",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.BackgroundPicture"] = "Background picture",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.BackgroundPicture.Hint"] = "The carousel background picture.",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.CustomUrl"] = "Custom url",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.CustomUrl.Hint"] = "The carousel custom url.",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.MaxProductsToShow"] = "Maximum products to show",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.MaxProductsToShow.Hint"] = "Specify the maximum number of products to show for this carousel.",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.ShowCountdown"] = "Show countdown",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.ShowCountdown.Hint"] = "Determine whether to show countdown in carousel.",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.DisplayOrder"] = "Display order",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.DisplayOrder.Hint"] = "Display order of the carousel. 1 represents the top of the list.",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.CreatedOn"] = "Created on",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.CreatedOn.Hint"] = "The create date of this carousel.",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.UpdatedOn"] = "Updated on",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.UpdatedOn.Hint"] = "The last update date of this carousel.",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.AclCustomerRoles"] = "Customer roles",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.AclCustomerRoles.Hint"] = "Select customer roles for which the category will be shown. Leave empty if you want this category to be visible to all users.",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.LimitedToStores"] = "Limited to stores",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.LimitedToStores.Hint"] = "Option to limit this carousel to a certain store. If you have multiple stores, choose one or several from the list. If you don't use this option just leave this field empty.",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.CustomCssClass"] = "Custom CSS class",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.CustomCssClass.Hint"] = "Enter the custom CSS class to be applied.",

            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.EnableAutoPlay"] = "Enable auto play",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.EnableAutoPlay.Hint"] = "Check to enable auto play.",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.AutoPlayTimeout"] = "Auto play timeout",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.AutoPlayTimeout.Hint"] = "It's autoplay interval timeout. (e.g 5000)",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.AutoPlayHoverPause"] = "Auto play hover pause",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.AutoPlayHoverPause.Hint"] = "Check to enable pause on mouse hover.",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.EnableLoop"] = "Enable loop",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.EnableLoop.Hint"] = "Check to enable loop.",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.StartPosition"] = "Start position",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.StartPosition.Hint"] = "The start position (e.g 0)",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.EnableKeyboardControl"] = "Enable keyboard control",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.EnableKeyboardControl.Hint"] = "Check to enable keyboard control.",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.KeyboardControlOnlyInViewport"] = "Only in viewport",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.KeyboardControlOnlyInViewport.Hint"] = "When enabled it will control sliders that are currently in viewport.",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.EnableNavigation"] = "Enable navigation",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.EnableNavigation.Hint"] = "Check to enable next/prev buttons.",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.EnableLazyLoad"] = "Enable lazy load",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.EnableLazyLoad.Hint"] = "Check to enable lazy load.",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.EnablePagination"] = "Enable pagination",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.EnablePagination.Hint"] = "Check to enable pagination.",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.PaginationType"] = "Pagination type",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.PaginationType.Hint"] = "Select pagination type.",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.PaginationDynamicMainBullets"] = "Dynamic main bullets",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.PaginationDynamicMainBullets.Hint"] = "The number of main bullets visible when dynamicBullets enabled.",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.PaginationDynamicBullets"] = "Dynamic bullets",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.PaginationDynamicBullets.Hint"] = "Good to enable if you use bullets pagination with a lot of slides. So it will keep only few bullets visible at the same time.",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.PaginationClickable"] = "Clickable",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.PaginationClickable.Hint"] = "If true then clicking on pagination button will cause transition to appropriate slide. Only for bullets pagination type.",

            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.Name.Required"] = "The name field is required.",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.Title.Required"] = "The title field is required.",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.MaxProductsToShow.Required"] = "The number of products to show field is required.",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.BackgroundPicture.Required"] = "The background picture field is required.",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.AvaliableDateTimeToUtc.Required"] = "Carousel end date is required.",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Fields.AvaliableDateTimeToUtc.Required.Hint"] = "Carousel end date (<a href=\"#carousel-wm-schedules\">Schedules</a> section) is required when 'Show countdown' is checked and 'Product source' is 'Custom Product'.",

            ["Admin.NopStation.SmartDealCarousels.Carousels.Products.AddNew"] = "Add new product",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Products.SaveBeforeEdit"] = "You need to save the carousel before you can add products for this carousel page.",

            ["Admin.NopStation.SmartDealCarousels.Carousels.Products.Fields.Product"] = "Product",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Products.Fields.Picture"] = "Picture",
            ["Admin.NopStation.SmartDealCarousels.Carousels.Products.Fields.DisplayOrder"] = "Display order",

            ["NopStation.SmartDealCarousels.LoadingFailed"] = "Failed to load carousel content.",
            ["NopStation.SmartDealCarousels.Countdown.Days"] = "Days",
            ["NopStation.SmartDealCarousels.Countdown.Hours"] = "Hours",
            ["NopStation.SmartDealCarousels.Countdown.Minutes"] = "Minutes",
            ["NopStation.SmartDealCarousels.Countdown.Seconds"] = "Seconds",
            ["NopStation.SmartDealCarousels.ImageLinkTitleFormat"] = "Picture of {0}",
            ["NopStation.SmartDealCarousels.ImageAlternateTextFormat"] = "Picture of {0}",
            ["NopStation.SmartDealCarousels.SeeMore"] = "See More"
        };

        return list.ToList();
    }
    #endregion
}
