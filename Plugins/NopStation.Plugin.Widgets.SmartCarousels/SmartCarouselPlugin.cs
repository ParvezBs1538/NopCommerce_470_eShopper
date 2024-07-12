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
using NopStation.Plugin.Widgets.SmartCarousels.Components;
using NopStation.Plugin.Widgets.SmartCarousels.Domains;

namespace NopStation.Plugin.Widgets.SmartCarousels;

public class SmartCarouselPlugin : BasePlugin, IWidgetPlugin, IAdminMenuPlugin, INopStationPlugin
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

    public SmartCarouselPlugin(IWebHelper webHelper,
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
        return $"{_webHelper.GetStoreLocation()}Admin/SmartCarousel/Configure";
    }

    public Type GetWidgetViewComponent(string widgetZone)
    {
        if (widgetZone == PublicWidgetZones.Footer)
            return typeof(SmartCarouselFooterViewComponent);

        return typeof(SmartCarouselViewComponent);
    }

    public async Task<IList<string>> GetWidgetZonesAsync()
    {
        var zones = (await _widgetZoneService.GetWidgetZonesForDomainAsync<SmartCarousel>()).ToList();
        if (!zones.Contains(PublicWidgetZones.Footer))
            zones.Add(PublicWidgetZones.Footer);

        return zones;
    }

    public async Task ManageSiteMapAsync(SiteMapNode rootNode)
    {
        if (await _permissionService.AuthorizeAsync(SmartCarouselPermissionProvider.ManageSmartCarousels))
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.SmartCarousels.Menu.SmartCarousel"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            var listItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.SmartCarousels.Menu.Carousels"),
                Url = "~/Admin/SmartCarousel/List",
                Visible = true,
                IconClass = "far fa-circle",
                SystemName = "SmartCarousels"
            };
            menuItem.ChildNodes.Add(listItem);

            var configItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.SmartCarousels.Menu.Configuration"),
                Url = "~/Admin/SmartCarousel/Configure",
                Visible = true,
                IconClass = "far fa-circle",
                SystemName = "SmartCarousels.Configuration"
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
        await _settingService.SaveSettingAsync(new SmartCarouselSettings
        {
            EnableAjaxLoad = true,
            EnableCarousel = true
        });

        await this.InstallPluginAsync(new SmartCarouselPermissionProvider());
        await base.InstallAsync();
    }

    public override async Task UninstallAsync()
    {
        await this.UninstallPluginAsync(new SmartCarouselPermissionProvider());
        await base.UninstallAsync();
    }

    public List<KeyValuePair<string, string>> PluginResouces()
    {
        var list = new Dictionary<string, string>
        {
            ["Admin.NopStation.SmartCarousels.Menu.SmartCarousel"] = "Carousel",
            ["Admin.NopStation.SmartCarousels.Menu.Configuration"] = "Configuration",
            ["Admin.NopStation.SmartCarousels.Menu.Carousels"] = "Carousels",

            ["Admin.NopStation.SmartCarousels.Configuration"] = "Carousel settings",
            ["Admin.NopStation.SmartCarousels.Configuration.Fields.EnableCarousel"] = "Enable carousel",
            ["Admin.NopStation.SmartCarousels.Configuration.Fields.EnableCarousel.Hint"] = "Check to enable smart carousel for your store.",
            ["Admin.NopStation.SmartCarousels.Configuration.Fields.EnableAjaxLoad"] = "Enable AJAX load",
            ["Admin.NopStation.SmartCarousels.Configuration.Fields.EnableAjaxLoad.Hint"] = "Determines whether carousels will load on AJAX.",

            ["Admin.NopStation.SmartCarousels.Carousels.List"] = "Carousels",
            ["Admin.NopStation.SmartCarousels.Carousels.List.SearchActive.Active"] = "Active",
            ["Admin.NopStation.SmartCarousels.Carousels.List.SearchActive.Inactive"] = "Inactive",

            ["Admin.NopStation.SmartCarousels.Carousels.List.SearchKeyword"] = "Keyword",
            ["Admin.NopStation.SmartCarousels.Carousels.List.SearchKeyword.Hint"] = "The search keyword.",
            ["Admin.NopStation.SmartCarousels.Carousels.List.SearchWidgetZone"] = "Widget zone",
            ["Admin.NopStation.SmartCarousels.Carousels.List.SearchWidgetZone.Hint"] = "The search widget zone.",
            ["Admin.NopStation.SmartCarousels.Carousels.List.SearchProductSource"] = "Product source",
            ["Admin.NopStation.SmartCarousels.Carousels.List.SearchProductSource.Hint"] = "The search product source.",
            ["Admin.NopStation.SmartCarousels.Carousels.List.SearchCarouselType"] = "Carousel type",
            ["Admin.NopStation.SmartCarousels.Carousels.List.SearchCarouselType.Hint"] = "The search carousel type.",
            ["Admin.NopStation.SmartCarousels.Carousels.List.SearchStore"] = "Store",
            ["Admin.NopStation.SmartCarousels.Carousels.List.SearchStore.Hint"] = "The search store.",
            ["Admin.NopStation.SmartCarousels.Carousels.List.SearchActive"] = "Active",
            ["Admin.NopStation.SmartCarousels.Carousels.List.SearchActive.Hint"] = "The search active.",

            ["Admin.NopStation.SmartCarousels.Carousels.Tab.Info"] = "Info",
            ["Admin.NopStation.SmartCarousels.Carousels.Tab.Properties"] = "Properties",
            ["Admin.NopStation.SmartCarousels.Carousels.Tab.Products"] = "Carousel products",
            ["Admin.NopStation.SmartCarousels.Carousels.Tab.Categories"] = "Carousel categories",
            ["Admin.NopStation.SmartCarousels.Carousels.Tab.Manufacturers"] = "Carousel manufacturers",
            ["Admin.NopStation.SmartCarousels.Carousels.Tab.Vendors"] = "Carousel vendors",
            ["Admin.NopStation.SmartCarousels.Carousels.Tab.Pictures"] = "Carousel pictures",

            ["Admin.NopStation.SmartCarousels.Carousels.EditDetails"] = "Edit carousel details",
            ["Admin.NopStation.SmartCarousels.Carousels.BackToList"] = "back to carousel list",
            ["Admin.NopStation.SmartCarousels.Carousels.AddNew"] = "Add new carousel",

            ["Admin.NopStation.SmartCarousels.Carousels.Products.AddNew"] = "Add new product",
            ["Admin.NopStation.SmartCarousels.Carousels.Products.SaveBeforeEdit"] = "You need to save the carousel before you can add products for this carousel page.",
            ["Admin.NopStation.SmartCarousels.Carousels.Categories.AddNew"] = "Add new category",
            ["Admin.NopStation.SmartCarousels.Carousels.Categories.SaveBeforeEdit"] = "You need to save the carousel before you can add categories for this carousel page.",
            ["Admin.NopStation.SmartCarousels.Carousels.Manufacturers.AddNew"] = "Add new manufacturer",
            ["Admin.NopStation.SmartCarousels.Carousels.Manufacturers.SaveBeforeEdit"] = "You need to save the carousel before you can add manufacturers for this carousel page.",
            ["Admin.NopStation.SmartCarousels.Carousels.Vendors.AddNew"] = "Add new vendor",
            ["Admin.NopStation.SmartCarousels.Carousels.Vendors.SaveBeforeEdit"] = "You need to save the carousel before you can add vendors for this carousel page.",
            ["Admin.NopStation.SmartCarousels.Carousels.Pictures.AddNew"] = "Add new picture",
            ["Admin.NopStation.SmartCarousels.Carousels.Pictures.SaveBeforeEdit"] = "You need to save the carousel before you can add pictures for this carousel page.",
            ["Admin.NopStation.SmartCarousels.Carousels.Pictures.EditDetails"] = "Edit picture details",

            ["Admin.NopStation.SmartCarousels.Carousels.Created"] = "Carousel has been created successfully.",
            ["Admin.NopStation.SmartCarousels.Carousels.Updated"] = "Carousel has been updated successfully.",
            ["Admin.NopStation.SmartCarousels.Carousels.Deleted"] = "Carousel has been deleted successfully.",

            ["Admin.NopStation.SmartCarousels.Carousels.Fields.Name"] = "Name",
            ["Admin.NopStation.SmartCarousels.Carousels.Fields.Name.Hint"] = "The carousel name.",
            ["Admin.NopStation.SmartCarousels.Carousels.Fields.Title"] = "Title",
            ["Admin.NopStation.SmartCarousels.Carousels.Fields.Title.Hint"] = "The carousel title.",
            ["Admin.NopStation.SmartCarousels.Carousels.Fields.DisplayTitle"] = "Display title",
            ["Admin.NopStation.SmartCarousels.Carousels.Fields.DisplayTitle.Hint"] = "Determines whether title should be displayed on public site (depends on theme design).",
            ["Admin.NopStation.SmartCarousels.Carousels.Fields.Active"] = "Active",
            ["Admin.NopStation.SmartCarousels.Carousels.Fields.Active.Hint"] = "Determines whether this carousel is active (visible on public store).",
            ["Admin.NopStation.SmartCarousels.Carousels.Fields.ProductSourceType"] = "Product source",
            ["Admin.NopStation.SmartCarousels.Carousels.Fields.ProductSourceType.Hint"] = "The product source for this carousel.",
            ["Admin.NopStation.SmartCarousels.Carousels.Fields.CarouselType"] = "Carousel type",
            ["Admin.NopStation.SmartCarousels.Carousels.Fields.CarouselType.Hint"] = "The carousel type.",
            ["Admin.NopStation.SmartCarousels.Carousels.Fields.ShowBackground"] = "Show background",
            ["Admin.NopStation.SmartCarousels.Carousels.Fields.ShowBackground.Hint"] = "Determines whether to show background.",
            ["Admin.NopStation.SmartCarousels.Carousels.Fields.BackgroundType"] = "Background type",
            ["Admin.NopStation.SmartCarousels.Carousels.Fields.BackgroundType.Hint"] = "The carousel background type.",
            ["Admin.NopStation.SmartCarousels.Carousels.Fields.BackgroundColor"] = "Background color",
            ["Admin.NopStation.SmartCarousels.Carousels.Fields.BackgroundColor.Hint"] = "The carousel background color.",
            ["Admin.NopStation.SmartCarousels.Carousels.Fields.BackgroundPicture"] = "Background picture",
            ["Admin.NopStation.SmartCarousels.Carousels.Fields.BackgroundPicture.Hint"] = "The carousel background picture.",
            ["Admin.NopStation.SmartCarousels.Carousels.Fields.CustomUrl"] = "Custom url",
            ["Admin.NopStation.SmartCarousels.Carousels.Fields.CustomUrl.Hint"] = "The carousel custom url.",
            ["Admin.NopStation.SmartCarousels.Carousels.Fields.MaxProductsToShow"] = "Maximum products to show",
            ["Admin.NopStation.SmartCarousels.Carousels.Fields.MaxProductsToShow.Hint"] = "Specify the maximum number of products to show for this carousel.",
            ["Admin.NopStation.SmartCarousels.Carousels.Fields.DisplayOrder"] = "Display order",
            ["Admin.NopStation.SmartCarousels.Carousels.Fields.DisplayOrder.Hint"] = "Display order of the carousel. 1 represents the top of the list.",
            ["Admin.NopStation.SmartCarousels.Carousels.Fields.CreatedOn"] = "Created on",
            ["Admin.NopStation.SmartCarousels.Carousels.Fields.CreatedOn.Hint"] = "The create date of this carousel.",
            ["Admin.NopStation.SmartCarousels.Carousels.Fields.UpdatedOn"] = "Updated on",
            ["Admin.NopStation.SmartCarousels.Carousels.Fields.UpdatedOn.Hint"] = "The last update date of this carousel.",
            ["Admin.NopStation.SmartCarousels.Carousels.Fields.AclCustomerRoles"] = "Customer roles",
            ["Admin.NopStation.SmartCarousels.Carousels.Fields.AclCustomerRoles.Hint"] = "Select customer roles for which the category will be shown. Leave empty if you want this category to be visible to all users.",
            ["Admin.NopStation.SmartCarousels.Carousels.Fields.LimitedToStores"] = "Limited to stores",
            ["Admin.NopStation.SmartCarousels.Carousels.Fields.LimitedToStores.Hint"] = "Option to limit this carousel to a certain store. If you have multiple stores, choose one or several from the list. If you don't use this option just leave this field empty.",
            ["Admin.NopStation.SmartCarousels.Carousels.Fields.CustomCssClass"] = "Custom CSS class",
            ["Admin.NopStation.SmartCarousels.Carousels.Fields.CustomCssClass.Hint"] = "Enter the custom CSS class to be applied.",

            ["Admin.NopStation.SmartCarousels.Carousels.Fields.EnableAutoPlay"] = "Enable auto play",
            ["Admin.NopStation.SmartCarousels.Carousels.Fields.EnableAutoPlay.Hint"] = "Check to enable auto play.",
            ["Admin.NopStation.SmartCarousels.Carousels.Fields.AutoPlayTimeout"] = "Auto play timeout",
            ["Admin.NopStation.SmartCarousels.Carousels.Fields.AutoPlayTimeout.Hint"] = "It's autoplay interval timeout. (e.g 5000)",
            ["Admin.NopStation.SmartCarousels.Carousels.Fields.AutoPlayHoverPause"] = "Auto play hover pause",
            ["Admin.NopStation.SmartCarousels.Carousels.Fields.AutoPlayHoverPause.Hint"] = "Check to enable pause on mouse hover.",
            ["Admin.NopStation.SmartCarousels.Carousels.Fields.EnableLoop"] = "Enable loop",
            ["Admin.NopStation.SmartCarousels.Carousels.Fields.EnableLoop.Hint"] = "Check to enable loop.",
            ["Admin.NopStation.SmartCarousels.Carousels.Fields.StartPosition"] = "Start position",
            ["Admin.NopStation.SmartCarousels.Carousels.Fields.StartPosition.Hint"] = "The start position (e.g 0)",
            ["Admin.NopStation.SmartCarousels.Carousels.Fields.Center"] = "Center",
            ["Admin.NopStation.SmartCarousels.Carousels.Fields.Center.Hint"] = "Check to center item. It works well with even and odd number of items.",
            ["Admin.NopStation.SmartCarousels.Carousels.Fields.EnableKeyboardControl"] = "Enable keyboard control",
            ["Admin.NopStation.SmartCarousels.Carousels.Fields.EnableKeyboardControl.Hint"] = "Check to enable keyboard control.",
            ["Admin.NopStation.SmartCarousels.Carousels.Fields.KeyboardControlOnlyInViewport"] = "Only in viewport",
            ["Admin.NopStation.SmartCarousels.Carousels.Fields.KeyboardControlOnlyInViewport.Hint"] = "When enabled it will control sliders that are currently in viewport.",
            ["Admin.NopStation.SmartCarousels.Carousels.Fields.EnableNavigation"] = "Enable navigation",
            ["Admin.NopStation.SmartCarousels.Carousels.Fields.EnableNavigation.Hint"] = "Check to enable next/prev buttons.",
            ["Admin.NopStation.SmartCarousels.Carousels.Fields.EnableLazyLoad"] = "Enable lazy load",
            ["Admin.NopStation.SmartCarousels.Carousels.Fields.EnableLazyLoad.Hint"] = "Check to enable lazy load.",
            ["Admin.NopStation.SmartCarousels.Carousels.Fields.EnablePagination"] = "Enable pagination",
            ["Admin.NopStation.SmartCarousels.Carousels.Fields.EnablePagination.Hint"] = "Check to enable pagination.",
            ["Admin.NopStation.SmartCarousels.Carousels.Fields.PaginationType"] = "Pagination type",
            ["Admin.NopStation.SmartCarousels.Carousels.Fields.PaginationType.Hint"] = "Select pagination type.",
            ["Admin.NopStation.SmartCarousels.Carousels.Fields.PaginationDynamicMainBullets"] = "Dynamic main bullets",
            ["Admin.NopStation.SmartCarousels.Carousels.Fields.PaginationDynamicMainBullets.Hint"] = "The number of main bullets visible when dynamicBullets enabled.",
            ["Admin.NopStation.SmartCarousels.Carousels.Fields.PaginationDynamicBullets"] = "Dynamic bullets",
            ["Admin.NopStation.SmartCarousels.Carousels.Fields.PaginationDynamicBullets.Hint"] = "Good to enable if you use bullets pagination with a lot of slides. So it will keep only few bullets visible at the same time.",
            ["Admin.NopStation.SmartCarousels.Carousels.Fields.PaginationClickable"] = "Clickable",
            ["Admin.NopStation.SmartCarousels.Carousels.Fields.PaginationClickable.Hint"] = "If true then clicking on pagination button will cause transition to appropriate slide. Only for bullets pagination type.",

            ["Admin.NopStation.SmartCarousels.Carousels.Pictures.Fields.Picture"] = "Picture",
            ["Admin.NopStation.SmartCarousels.Carousels.Pictures.Fields.Picture.Hint"] = "The picture.",
            ["Admin.NopStation.SmartCarousels.Carousels.Pictures.Fields.Label"] = "Label",
            ["Admin.NopStation.SmartCarousels.Carousels.Pictures.Fields.Label.Hint"] = "The label.",
            ["Admin.NopStation.SmartCarousels.Carousels.Pictures.Fields.RedirectUrl"] = "Redirect URL",
            ["Admin.NopStation.SmartCarousels.Carousels.Pictures.Fields.RedirectUrl.Hint"] = "The redirect URL.",
            ["Admin.NopStation.SmartCarousels.Carousels.Pictures.Fields.DisplayOrder"] = "Display order",
            ["Admin.NopStation.SmartCarousels.Carousels.Pictures.Fields.DisplayOrder.Hint"] = "Display order of the carousel picture. 1 represents the top of the list.",
            ["Admin.NopStation.SmartCarousels.Carousels.Pictures.Fields.OverrideAltAttribute"] = "Alt",
            ["Admin.NopStation.SmartCarousels.Carousels.Pictures.Fields.OverrideAltAttribute.Hint"] = "Override \"alt\" attribute for \"img\" HTML element. If empty, then a default rule will be used (e.g. carousel name).",
            ["Admin.NopStation.SmartCarousels.Carousels.Pictures.Fields.OverrideTitleAttribute"] = "Title",
            ["Admin.NopStation.SmartCarousels.Carousels.Pictures.Fields.OverrideTitleAttribute.Hint"] = "Override \"title\" attribute for \"img\" HTML element. If empty, then a default rule will be used (e.g. carousel name).",

            ["Admin.NopStation.SmartCarousels.Carousels.Pictures.Fields.Picture.Required"] = "The picture field is required.",
            ["Admin.NopStation.SmartCarousels.Carousels.Pictures.Fields.Label.Required"] = "The label field is required.",

            ["Admin.NopStation.SmartCarousels.Carousels.Categories.Fields.Category"] = "Category",
            ["Admin.NopStation.SmartCarousels.Carousels.Categories.Fields.DisplayOrder"] = "Display order",
            ["Admin.NopStation.SmartCarousels.Carousels.Manufacturers.Fields.Manufacturer"] = "Manufacturer",
            ["Admin.NopStation.SmartCarousels.Carousels.Manufacturers.Fields.DisplayOrder"] = "Display order",
            ["Admin.NopStation.SmartCarousels.Carousels.Products.Fields.Product"] = "Product",
            ["Admin.NopStation.SmartCarousels.Carousels.Products.Fields.Picture"] = "Picture",
            ["Admin.NopStation.SmartCarousels.Carousels.Products.Fields.DisplayOrder"] = "Display order",
            ["Admin.NopStation.SmartCarousels.Carousels.Vendors.Fields.Vendor"] = "Vendor",
            ["Admin.NopStation.SmartCarousels.Carousels.Vendors.Fields.DisplayOrder"] = "Display order",

            ["Admin.NopStation.SmartCarousels.Carousels.Fields.Name.Required"] = "The name field is required.",
            ["Admin.NopStation.SmartCarousels.Carousels.Fields.Title.Required"] = "The title field is required.",
            ["Admin.NopStation.SmartCarousels.Carousels.Fields.MaxProductsToShow.Required"] = "The number of products to show field is required.",
            ["Admin.NopStation.SmartCarousels.Carousels.Fields.BackgroundPicture.Required"] = "The background picture field is required.",

            ["NopStation.SmartCarousels.LoadingFailed"] = "Failed to load carousel content.",
            ["NopStation.SmartCarousels.SeeMore"] = "See More"
        };

        return list.ToList();
    }
    #endregion
}
