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
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Misc.WidgetManager.Services;
using NopStation.Plugin.Widgets.SmartMegaMenu.Components;
using NopStation.Plugin.Widgets.SmartMegaMenu.Domain;

namespace NopStation.Plugin.Widgets.SmartMegaMenu;

public class SmartMegaMenuPlugin : BasePlugin, IWidgetPlugin, IAdminMenuPlugin, INopStationPlugin
{
    #region Fields

    private readonly IWebHelper _webHelper;
    private readonly INopStationCoreService _nopStationCoreService;
    private readonly ILocalizationService _localizationService;
    private readonly IPermissionService _permissionService;
    private readonly ISettingService _settingService;
    private readonly IWidgetZoneService _widgetZoneService;

    public bool HideInWidgetList => false;

    #endregion

    #region Ctor

    public SmartMegaMenuPlugin(IWebHelper webHelper,
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
        return _webHelper.GetStoreLocation() + "Admin/SmartMegaMenu/Configure";
    }

    public override async Task InstallAsync()
    {
        var settings = new SmartMegaMenuSettings()
        {
            EnableMegaMenu = true,
            HideDefaultMenu = true,
            MenuItemPictureSize = 300
        };
        await _settingService.SaveSettingAsync(settings);

        await this.InstallPluginAsync(new SmartMegaMenuPermissionProvider());
        await base.InstallAsync();
    }

    public override async Task UninstallAsync()
    {
        await this.UninstallPluginAsync(new SmartMegaMenuPermissionProvider());
        await base.UninstallAsync();
    }

    public async Task ManageSiteMapAsync(SiteMapNode rootNode)
    {
        if (await _permissionService.AuthorizeAsync(SmartMegaMenuPermissionProvider.ManageMegaMenu))
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.SmartMegaMenu.Menu.SmartMegaMenu"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            var categoryIcon = new SiteMapNode()
            {
                Visible = true,
                IconClass = "far fa-circle",
                Url = "~/Admin/SmartMegaMenu/List",
                SystemName = "SmartMegaMenu.Menus",
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.SmartMegaMenu.Menu.MegaMenus"),
            };
            menuItem.ChildNodes.Add(categoryIcon);

            var configItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.SmartMegaMenu.Menu.Configuration"),
                Url = "~/Admin/SmartMegaMenu/Configure",
                Visible = true,
                IconClass = "far fa-circle",
                SystemName = "SmartMegaMenu.Configuration"
            };
            menuItem.ChildNodes.Add(configItem);

            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/smart-mega-menu-documentation",
                    Visible = true,
                    IconClass = "far fa-circle",
                    OpenUrlInNewTab = true
                };
                menuItem.ChildNodes.Add(documentation);
            }

            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menuItem, NopStationMenuType.Plugin);
        }
    }

    public List<KeyValuePair<string, string>> PluginResouces()
    {
        var list = new Dictionary<string, string>
        {
            ["Admin.NopStation.SmartMegaMenu.Menu.SmartMegaMenu"] = "Mega menu",
            ["Admin.NopStation.SmartMegaMenu.Menu.Configuration"] = "Configuration",
            ["Admin.NopStation.SmartMegaMenu.Menu.MegaMenus"] = "Mega menus",

            ["Admin.NopStation.SmartMegaMenu.Configuration.Fields.EnableMegaMenu"] = "Enable mega menu",
            ["Admin.NopStation.SmartMegaMenu.Configuration.Fields.EnableMegaMenu.Hint"] = "Determines whether to enable mega menu.",
            ["Admin.NopStation.SmartMegaMenu.Configuration.Fields.HideDefaultMenu"] = "Hide default menu",
            ["Admin.NopStation.SmartMegaMenu.Configuration.Fields.HideDefaultMenu.Hint"] = "Determines whether to hide default menu.",
            ["Admin.NopStation.SmartMegaMenu.Configuration.Fields.MenuItemPictureSize"] = "Menu item picture size",
            ["Admin.NopStation.SmartMegaMenu.Configuration.Fields.MenuItemPictureSize.Hint"] = "The picture size in menu item.",
            ["Admin.NopStation.SmartMegaMenu.Configuration"] = "Mega menu settings",

            ["Admin.NopStation.SmartMegaMenu.MegaMenus.EditDetails"] = "Edit menu details",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.BackToList"] = "back to menu list",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.AddNew"] = "Add new menu",

            ["Admin.NopStation.SmartMegaMenu.MegaMenus.List"] = "Mega menus",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.List.SearchActive.Active"] = "Active",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.List.SearchActive.Inactive"] = "Inactive",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.List.SearchStore"] = "Store",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.List.SearchStore.Hint"] = "The search store.",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.List.SearchActive"] = "Active",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.List.SearchActive.Hint"] = "The search active status.",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.List.SearchKeyword"] = "Keyword",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.List.SearchKeyword.Hint"] = "The search keyword.",

            ["Admin.NopStation.SmartMegaMenu.MegaMenus.Tab.Info"] = "Info",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.Tab.MenuItems"] = "Menu items",

            ["Admin.NopStation.SmartMegaMenu.MegaMenus.Created"] = "Mega menu has been created successfully.",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.Updated"] = "Mega menu has been updated successfully.",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.Deleted"] = "Mega menu has been deleted successfully.",

            ["Admin.NopStation.SmartMegaMenu.MegaMenus.Fields.Name"] = "Name",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.Fields.Name.Hint"] = "Name for the menu",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.Fields.Name.Required"] = "The mega menu name is required.",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.Fields.Active"] = "Active",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.Fields.Active.Hint"] = "Determines whether to enable this menu.",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.Fields.ViewType"] = "View type",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.Fields.ViewType.Hint"] = "View orientation type",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.Fields.WithoutImages"] = "Without images",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.Fields.WithoutImages.Hint"] = "Hide all images from the menu",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.Fields.DisplayOrder"] = "Display order",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.Fields.DisplayOrder.Hint"] = "Display order of the mega menu. 1 represents the top of the list.",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.Fields.LimitedToStores"] = "Limited to stores",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.Fields.LimitedToStores.Hint"] = "Option to limit this mega menu to a certain store. If you have multiple stores, choose one or several from the list. If you don't use this option just leave this field empty.",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.Fields.UpdatedOn"] = "Updated on",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.Fields.UpdatedOn.Hint"] = "The update date of this mega menu.",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.Fields.CreatedOn"] = "Created on",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.Fields.CreatedOn.Hint"] = "The create date of this mega menu.",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.Fields.CssClass"] = "CSS class",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.Fields.CssClass.Hint"] = "Custom css class to change the menu style",

            ["Admin.NopStation.SmartMegaMenu.MegaMenus.Navigation"] = "Navigation",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.Navigation.Hint"] = "Drag and drop to modify order, orientation or properties for the mega menu items.",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.MenuOptions"] = "Menu options",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.MenuOptions.Hint"] = "Select offered options to construct the mega menu.",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.MenuOptions.Select"] = "Select",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.MenuOptions.Add"] = "Add",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.MenuOptions.Categories"] = "Categories",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.MenuOptions.Manufacturers"] = "Manufacturers",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.MenuOptions.Vendors"] = "Vendors",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.MenuOptions.Topics"] = "Topics",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.MenuOptions.ProductTags"] = "Product tags",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.MenuOptions.OtherPages"] = "Other pages",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.MenuOptions.CustomLinks"] = "Custom links",

            ["Admin.NopStation.SmartMegaMenu.MegaMenus.MenuOptions.CustomLinks.AddButton"] = "Add custom link",

            ["Admin.NopStation.SmartMegaMenu.MegaMenus.ProductTags.Fields.Name"] = "Name",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.OtherPages.Fields.Name"] = "Name",

            ["Enums.NopStation.Plugin.Widgets.SmartMegaMenu.Domain.PageType.HomePage"] = "Home Page",
            ["Enums.NopStation.Plugin.Widgets.SmartMegaMenu.Domain.PageType.ContactUs"] = "Contact Us",
            ["Enums.NopStation.Plugin.Widgets.SmartMegaMenu.Domain.PageType.MyAccount"] = "My Account",
            ["Enums.NopStation.Plugin.Widgets.SmartMegaMenu.Domain.PageType.Login"] = "Login",
            ["Enums.NopStation.Plugin.Widgets.SmartMegaMenu.Domain.PageType.Register"] = "Register",
            ["Enums.NopStation.Plugin.Widgets.SmartMegaMenu.Domain.PageType.NewProducts"] = "New Products",
            ["Enums.NopStation.Plugin.Widgets.SmartMegaMenu.Domain.PageType.CompareProducts"] = "Compare Products",
            ["Enums.NopStation.Plugin.Widgets.SmartMegaMenu.Domain.PageType.RecentlyViewedProducts"] = "Recently Viewed Products",
            ["Enums.NopStation.Plugin.Widgets.SmartMegaMenu.Domain.PageType.Manufacturers"] = "Manufacturers",
            ["Enums.NopStation.Plugin.Widgets.SmartMegaMenu.Domain.PageType.Vendors"] = "Vendors",
            ["Enums.NopStation.Plugin.Widgets.SmartMegaMenu.Domain.PageType.ProductTags"] = "Product Tags",
            ["Enums.NopStation.Plugin.Widgets.SmartMegaMenu.Domain.PageType.Search"] = "Search",
            ["Enums.NopStation.Plugin.Widgets.SmartMegaMenu.Domain.PageType.Cart"] = "Cart",
            ["Enums.NopStation.Plugin.Widgets.SmartMegaMenu.Domain.PageType.Wishlist"] = "Wishlist",
            ["Enums.NopStation.Plugin.Widgets.SmartMegaMenu.Domain.PageType.Blog"] = "Blog",
            ["Enums.NopStation.Plugin.Widgets.SmartMegaMenu.Domain.PageType.News"] = "News",
            ["Enums.NopStation.Plugin.Widgets.SmartMegaMenu.Domain.PageType.Forums"] = "Forums",
            ["Enums.NopStation.Plugin.Widgets.SmartMegaMenu.Domain.PageType.Checkout"] = "Checkout",
            ["Enums.NopStation.Plugin.Widgets.SmartMegaMenu.Domain.PageType.Sitemap"] = "Sitemap",
            ["Enums.NopStation.Plugin.Widgets.SmartMegaMenu.Domain.PageType.ApplyVendor"] = "Apply Vendor",
            ["Enums.NopStation.Plugin.Widgets.SmartMegaMenu.Domain.PageType.PrivateMessages"] = "Private Messages",

            ["Admin.NopStation.SmartMegaMenu.MegaMenus.MenuItems.Fields.Title"] = "Title",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.MenuItems.Fields.Title.Hint"] = "The menu item title.",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.MenuItems.Fields.Url"] = "Url",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.MenuItems.Fields.Url.Hint"] = "The menu item url.",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.MenuItems.Fields.OpenInNewTab"] = "Open in new tab",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.MenuItems.Fields.OpenInNewTab.Hint"] = "Determines whether to open page in new tab.",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.MenuItems.Fields.CssClass"] = "CSS class",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.MenuItems.Fields.CssClass.Hint"] = "The custom CSS class.",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.MenuItems.Fields.ShowRibbonText"] = "Show ribbon text",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.MenuItems.Fields.ShowRibbonText.Hint"] = "Determines whether to show ribbon text.",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.MenuItems.Fields.RibbonText"] = "Ribbon text",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.MenuItems.Fields.RibbonText.Hint"] = "The ribbon text for menu item.",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.MenuItems.Fields.RibbonBackgroundColor"] = "Ribbon background color",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.MenuItems.Fields.RibbonBackgroundColor.Hint"] = "The ribbon background color for menu item.",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.MenuItems.Fields.RibbonTextColor"] = "Ribbon text color",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.MenuItems.Fields.RibbonTextColor.Hint"] = "The ribbon text color for menu item.",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.MenuItems.Fields.ShowPicture"] = "Show picture",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.MenuItems.Fields.ShowPicture.Hint"] = "Determines whether to show picture in menu item.",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.MenuItems.Fields.Picture"] = "Picture",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.MenuItems.Fields.Picture.Hint"] = "The picture.",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.MenuItems.Fields.DisplayOrder"] = "Display order",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.MenuItems.Fields.DisplayOrder.Hint"] = "Display order of the menu item. 1 represents the top of the list.",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.MenuItems.Fields.AclCustomerRoles"] = "Customer roles",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.MenuItems.Fields.AclCustomerRoles.Hint"] = "Select customer roles for which the menu item will be shown. Leave empty if you want this menu item to be visible to all users.",

            ["Admin.NopStation.SmartMegaMenu.MegaMenus.MenuItems.Fields.Title.Required"] = "The menu item title is required.",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.MenuItems.Fields.Url.Required"] = "The menu item URL is required.",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.MenuItems.Fields.RibbonText.Required"] = "The menu item ribbon text is required.",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.MenuOptions.CustomLinks.Title.Alert.AddNew"] = "Enter menu item title",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.MenuOptions.CustomLinks.Url.Alert.AddNew"] = "Enter menu item URL",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.MenuOptions.CustomLinks.Alert.CustomLinkAdd"] = "Failed to add menu item",

            ["Admin.NopStation.SmartMegaMenu.Close"] = "Close",
            ["Admin.NopStation.SmartMegaMenu.MegaMenus.MenuItems.SaveBeforeEdit"] = "You need to save the mega menu before you can add pictures for this mega menu page.",

            ["NopStation.SmartMegaMenu.Back"] = "Back",
            ["NopStation.SmartMegaMenu.Image.AlterText"] = "The picture of {0}",
            ["NopStation.SmartMegaMenu.Image.Title"] = "The picture of {0}",
        };

        return list.ToList();
    }

    public async Task<IList<string>> GetWidgetZonesAsync()
    {
        var widgetZones = await _widgetZoneService.GetWidgetZonesForDomainAsync<MegaMenu>();
        return widgetZones;
    }

    public Type GetWidgetViewComponent(string widgetZone)
    {
        return typeof(SmartMegaMenuViewComponent);
    }

    #endregion
}
