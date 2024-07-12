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
using NopStation.Plugin.Widgets.Popups.Components;

namespace NopStation.Plugin.Widgets.Popups;

public class PopupPlugin : BasePlugin, IWidgetPlugin, IAdminMenuPlugin, INopStationPlugin
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

    public PopupPlugin(IWebHelper webHelper,
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
        return $"{_webHelper.GetStoreLocation()}Admin/Popup/Configure";
    }

    public async Task ManageSiteMapAsync(SiteMapNode rootNode)
    {
        var menuItem = new SiteMapNode()
        {
            Title = await _localizationService.GetResourceAsync("Admin.NopStation.Popups.Menu.Popup"),
            Visible = true,
            IconClass = "far fa-dot-circle",
        };

        if (await _permissionService.AuthorizeAsync(PopupPermissionProvider.ManagePopup))
        {
            var listItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Popups.Menu.Popup.List"),
                Url = "~/Admin/Popup/List",
                Visible = true,
                IconClass = "far fa-circle",
                SystemName = "Popups"
            };
            menuItem.ChildNodes.Add(listItem);

            var configItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Popups.Menu.Configuration"),
                Url = "~/Admin/Popup/Configure",
                Visible = true,
                IconClass = "far fa-circle",
                SystemName = "Popup.Configuration"
            };
            menuItem.ChildNodes.Add(configItem);
        }

        if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
        {
            var documentation = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                Url = "https://www.nop-station.com/popup-documentation?utm_source=admin-panel?utm_source=admin-panel&utm_medium=products&utm_campaign=popup",
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
        await _settingService.SaveSettingAsync(new PopupSettings()
        {
            NewsletterPopupTitle = "Want to Get Discount & Product Updates From Us?",
            NewsletterPopupDesctiption = "<p>Don’t miss any updates of our new Apps, Themes, Plugins and all the astonishing offers we bring for you.</p>",
            PopupOpenerSelector = "#newsletter-subscribe-button",
            AllowCustomerToSelectDoNotShowThisPopupAgain = true,
            PreSelectedDoNotShowThisPopupAgain = true,
            DelayTime = 2000,
            EnableNewsletterPopup = true,
            OpenPopupOnLoadPage = true,
            ShowOnHomePageOnly = true
        });

        await this.InstallPluginAsync(new PopupPermissionProvider());
        await base.InstallAsync();
    }

    public override async Task UninstallAsync()
    {
        await this.UninstallPluginAsync(new PopupPermissionProvider());
        await base.UninstallAsync();
    }

    public List<KeyValuePair<string, string>> PluginResouces()
    {
        var list = new Dictionary<string, string>
        {
            ["Admin.NopStation.Popups.Menu.Popup"] = "Popup",
            ["Admin.NopStation.Popups.Menu.Popup.List"] = "Popups",
            ["Admin.NopStation.Popups.Menu.Configuration"] = "Configuration",

            ["Admin.NopStation.Popups.Configuration.Fields.EnableNewsletterPopup"] = "Enable newsletter popup",
            ["Admin.NopStation.Popups.Configuration.Fields.EnableNewsletterPopup.Hint"] = "Check to enable newsletter popup for your store.",
            ["Admin.NopStation.Popups.Configuration.Fields.NewsletterPopupTitle"] = "Newsletter popup title",
            ["Admin.NopStation.Popups.Configuration.Fields.NewsletterPopupTitle.Hint"] = "Define newsletter popup title for your store.",
            ["Admin.NopStation.Popups.Configuration.Fields.NewsletterPopupDesctiption"] = "Newsletter popup desctiption",
            ["Admin.NopStation.Popups.Configuration.Fields.NewsletterPopupDesctiption.Hint"] = "Define newsletter desctiption title for your store.",
            ["Admin.NopStation.Popups.Configuration.Fields.BackgroundPicture"] = "Background picture",
            ["Admin.NopStation.Popups.Configuration.Fields.BackgroundPicture.Hint"] = "Popup background picture.",
            ["Admin.NopStation.Popups.Configuration.Fields.RedirectUrl"] = "Redirect URL",
            ["Admin.NopStation.Popups.Configuration.Fields.RedirectUrl.Hint"] = "Define redirect URL.",
            ["Admin.NopStation.Popups.Configuration.Fields.PopupOpenerSelector"] = "Popup opener selector",
            ["Admin.NopStation.Popups.Configuration.Fields.PopupOpenerSelector.Hint"] = "Define popup opener selector.",
            ["Admin.NopStation.Popups.Configuration.Fields.DelayTime"] = "Delay time",
            ["Admin.NopStation.Popups.Configuration.Fields.DelayTime.Hint"] = "Define popup opener delay time in mili seconds (i.e. 2000 for 2 seconds).",
            ["Admin.NopStation.Popups.Configuration.Fields.OpenPopupOnLoadPage"] = "Open popup on load page",
            ["Admin.NopStation.Popups.Configuration.Fields.OpenPopupOnLoadPage.Hint"] = "Mark as checked to open newsletter popup on load page.",
            ["Admin.NopStation.Popups.Configuration.Fields.AllowCustomerToSelectDoNotShowThisPopupAgain"] = "Allow customer to select \"Do not show this popup again\"",
            ["Admin.NopStation.Popups.Configuration.Fields.AllowCustomerToSelectDoNotShowThisPopupAgain.Hint"] = "Mark as checked to allow customer to select \"Do not show this popup again\".",
            ["Admin.NopStation.Popups.Configuration.Fields.PreSelectedDoNotShowThisPopupAgain"] = "Pre-selected \"Do not show this popup again\"",
            ["Admin.NopStation.Popups.Configuration.Fields.PreSelectedDoNotShowThisPopupAgain.Hint"] = "Define whether to pre-select \"Do not show this popup again\".",
            ["Admin.NopStation.Popups.Configuration.Fields.ShowOnHomePageOnly"] = "Show on home page only",
            ["Admin.NopStation.Popups.Configuration.Fields.ShowOnHomePageOnly.Hint"] = "This default newsletter popup will be appear on home page only.",
            ["Admin.NopStation.Popups.Configuration"] = "Popup settings",

            ["Admin.NopStation.Popups.Popups.Created"] = "Popup has been created successfully.",
            ["Admin.NopStation.Popups.Popups.Updated"] = "Popup has been updated successfully.",
            ["Admin.NopStation.Popups.Popups.Deleted"] = "Popup has been deleted successfully.",

            ["Admin.NopStation.Popups.Popups.Fields.Name"] = "Name",
            ["Admin.NopStation.Popups.Popups.Fields.Name.Hint"] = "Popup name.",
            ["Admin.NopStation.Popups.Popups.Fields.Name.Required"] = "The popup name is required.",
            ["Admin.NopStation.Popups.Popups.Fields.ColumnTypeId"] = "Column type",
            ["Admin.NopStation.Popups.Popups.Fields.ColumnTypeId.Hint"] = "Popup column type.",
            ["Admin.NopStation.Popups.Popups.Fields.DeviceTypeId"] = "Device type",
            ["Admin.NopStation.Popups.Popups.Fields.DeviceTypeId.Hint"] = "Popup device type.",
            ["Admin.NopStation.Popups.Popups.Fields.Column1ContentTypeId"] = "Content type",
            ["Admin.NopStation.Popups.Popups.Fields.Column1ContentTypeId.Hint"] = "Popup column 1 content type.",
            ["Admin.NopStation.Popups.Popups.Fields.Column1Text"] = "Text",
            ["Admin.NopStation.Popups.Popups.Fields.Column1Text.Hint"] = "Popup column 1 text.",
            ["Admin.NopStation.Popups.Popups.Fields.Column1DesktopPictureId"] = "Desktop picture",
            ["Admin.NopStation.Popups.Popups.Fields.Column1DesktopPictureId.Hint"] = "Popup column 1 picture for desktop view.",
            ["Admin.NopStation.Popups.Popups.Fields.Column1MobilePictureId"] = "Mobile picture",
            ["Admin.NopStation.Popups.Popups.Fields.Column1MobilePictureId.Hint"] = "Popup column 1 picture for mobile view.",
            ["Admin.NopStation.Popups.Popups.Fields.Column1PopupUrl"] = "Popup URL",
            ["Admin.NopStation.Popups.Popups.Fields.Column1PopupUrl.Hint"] = "Popup column 1 redirect URL.",
            ["Admin.NopStation.Popups.Popups.Fields.Column2ContentTypeId"] = "Content type",
            ["Admin.NopStation.Popups.Popups.Fields.Column2ContentTypeId.Hint"] = "Popup column 2 content type.",
            ["Admin.NopStation.Popups.Popups.Fields.Column2Text"] = "Text",
            ["Admin.NopStation.Popups.Popups.Fields.Column2Text.Hint"] = "Popup column 2 text.",
            ["Admin.NopStation.Popups.Popups.Fields.Column2DesktopPictureId"] = "Desktop picture",
            ["Admin.NopStation.Popups.Popups.Fields.Column2DesktopPictureId.Hint"] = "Popup column 2 picture for desktop view.",
            ["Admin.NopStation.Popups.Popups.Fields.Column2MobilePictureId"] = "Mobile picture",
            ["Admin.NopStation.Popups.Popups.Fields.Column2MobilePictureId.Hint"] = "Popup column 2 picture for mobile view.",
            ["Admin.NopStation.Popups.Popups.Fields.Column2PopupUrl"] = "Popup URL",
            ["Admin.NopStation.Popups.Popups.Fields.Column2PopupUrl.Hint"] = "Popup column 2 redirect URL.",
            ["Admin.NopStation.Popups.Popups.Fields.EnableStickyButton"] = "Enable sticky button",
            ["Admin.NopStation.Popups.Popups.Fields.EnableStickyButton.Hint"] = "Check to enable sticky button.",
            ["Admin.NopStation.Popups.Popups.Fields.StickyButtonText"] = "Sticky button text",
            ["Admin.NopStation.Popups.Popups.Fields.StickyButtonText.Hint"] = "The popup sticky button text.",
            ["Admin.NopStation.Popups.Popups.Fields.StickyButtonColor"] = "Sticky button color",
            ["Admin.NopStation.Popups.Popups.Fields.StickyButtonColor.Hint"] = "The popup sticky button color.",
            ["Admin.NopStation.Popups.Popups.Fields.StickyButtonPositionId"] = "Sticky button position",
            ["Admin.NopStation.Popups.Popups.Fields.StickyButtonPositionId.Hint"] = "The popup sticky button position.",
            ["Admin.NopStation.Popups.Popups.Fields.CssClass"] = "CSS class",
            ["Admin.NopStation.Popups.Popups.Fields.CssClass.Hint"] = "Popup custom CSS class.",
            ["Admin.NopStation.Popups.Popups.Fields.DelayTime"] = "Delay time",
            ["Admin.NopStation.Popups.Popups.Fields.DelayTime.Hint"] = "Define popup opener delay time in milliseconds (i.e. 2000 for 2 seconds).",
            ["Admin.NopStation.Popups.Popups.Fields.OpenPopupOnLoadPage"] = "Open popup on load page",
            ["Admin.NopStation.Popups.Popups.Fields.OpenPopupOnLoadPage.Hint"] = "Mark as checked to open popup on load page.",
            ["Admin.NopStation.Popups.Popups.Fields.AllowCustomerToSelectDoNotShowThisPopupAgain"] = "Allow customer to select \"Do not show this popup again\"",
            ["Admin.NopStation.Popups.Popups.Fields.AllowCustomerToSelectDoNotShowThisPopupAgain.Hint"] = "Mark as checked to allow customer to select \"Do not show this popup again\".",
            ["Admin.NopStation.Popups.Popups.Fields.PreSelectedDoNotShowThisPopupAgain"] = "Pre-selected \"Do not show this popup again\"",
            ["Admin.NopStation.Popups.Popups.Fields.PreSelectedDoNotShowThisPopupAgain.Hint"] = "Define whether to pre-select \"Do not show this popup again\".",
            ["Admin.NopStation.Popups.Popups.Fields.LimitedToStores"] = "Limited to stores",
            ["Admin.NopStation.Popups.Popups.Fields.LimitedToStores.Hint"] = "Option to limit this popup to a certain store. If you have multiple stores, choose one or several from the list. If you don't use this option just leave this field empty. In order to use this functionality, you have to disable the following setting: Configuration &gt; Catalog settings &gt; Ignore \"limit per store\" rules (sitewide).",
            ["Admin.NopStation.Popups.Popups.Fields.Active"] = "Active",
            ["Admin.NopStation.Popups.Popups.Fields.Active.Hint"] = "Determines whether this popup is active (visible on public store).",
            ["Admin.NopStation.Popups.Popups.Fields.CreatedOn"] = "Created on",
            ["Admin.NopStation.Popups.Popups.Fields.CreatedOn.Hint"] = "Popup create date.",
            ["Admin.NopStation.Popups.Popups.Fields.AclCustomerRoles"] = "Customer roles",
            ["Admin.NopStation.Popups.Popups.Fields.AclCustomerRoles.Hint"] = "Choose one or several customer roles i.e. administrators, vendors, guests, who will be able to see this popup in catalog. If you don't need this option just leave this field empty. In order to use this functionality, you have to disable the following setting: Configuration &gt; Settings &gt; Catalog &gt; Ignore ACL rules (sitewide).",

            ["Admin.NopStation.Popups.Popups.Column1"] = "Column 1",
            ["Admin.NopStation.Popups.Popups.Column2"] = "Column 2",

            ["Admin.NopStation.Popups.Popups.List.SearchStore"] = "Store",
            ["Admin.NopStation.Popups.Popups.List.SearchStore.Hint"] = "The search store",
            ["Admin.NopStation.Popups.Popups.List.SearchActive"] = "Active",
            ["Admin.NopStation.Popups.Popups.List.SearchActive.Hint"] = "Search by active property.",
            ["Admin.NopStation.Popups.Popups.List.SearchActive.Active"] = "Active",
            ["Admin.NopStation.Popups.Popups.List.SearchActive.Inactive"] = "Inactive",
            ["Admin.NopStation.Popups.Popups.List"] = "Popups",
            ["Admin.NopStation.Popups.Tabs.Info"] = "Info",
            ["Admin.NopStation.Popups.Tabs.Contents"] = "Contents",
            ["Admin.NopStation.Popups.Popups.AddNew"] = "Add new popup",
            ["Admin.NopStation.Popups.Popups.EditDetails"] = "Edit popup",
            ["Admin.NopStation.Popups.Popups.BackToList"] = "back to popup list",

            ["NopStation.Popups.DefaultPopup.ImageLinkTitleFormat"] = "Picture of newsletter subscription.",
            ["NopStation.Popups.DefaultPopup.ImageAlternateTextFormat"] = "Picture of newsletter subscription.",
            ["NopStation.Popups.Popups.ImageLinkTitleFormat"] = "Picture of {0}.",
            ["NopStation.Popups.Popups.ImageAlternateTextFormat"] = "Picture of {0}.",
            ["NopStation.Popups.Popups.HideThisPopup"] = "Do not show this popup again",
        };

        return list.ToList();
    }

    public async Task<IList<string>> GetWidgetZonesAsync()
    {
        return await Task.FromResult(new List<string>()
        {
            PublicWidgetZones.Footer
        });
    }

    public Type GetWidgetViewComponent(string widgetZone)
    {
        return typeof(PopupViewComponent);
    }

    #endregion
}
