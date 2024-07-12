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
using NopStation.Plugin.Widgets.SmartSliders.Components;
using NopStation.Plugin.Widgets.SmartSliders.Domains;

namespace NopStation.Plugin.Widgets.SmartSliders;

public class SmartSliderPlugin : BasePlugin, IWidgetPlugin, IAdminMenuPlugin, INopStationPlugin
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

    public SmartSliderPlugin(IWebHelper webHelper,
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
        return $"{_webHelper.GetStoreLocation()}Admin/SmartSlider/Configure";
    }

    public Type GetWidgetViewComponent(string widgetZone)
    {
        if (widgetZone == PublicWidgetZones.Footer)
            return typeof(SmartSliderFooterViewComponent);

        return typeof(SmartSliderViewComponent);
    }

    public async Task<IList<string>> GetWidgetZonesAsync()
    {
        var zones = (await _widgetZoneService.GetWidgetZonesForDomainAsync<SmartSlider>()).ToList();
        if (!zones.Contains(PublicWidgetZones.Footer))
            zones.Add(PublicWidgetZones.Footer);

        return zones;
    }

    public async Task ManageSiteMapAsync(SiteMapNode rootNode)
    {
        var menuItem = new SiteMapNode()
        {
            Title = await _localizationService.GetResourceAsync("Admin.NopStation.SmartSliders.Menu.SmartSlider"),
            Visible = true,
            IconClass = "far fa-dot-circle",
        };

        if (await _permissionService.AuthorizeAsync(SmartSliderPermissionProvider.ManageSliders))
        {
            var listItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.SmartSliders.Menu.Sliders"),
                Url = "~/Admin/SmartSlider/List",
                Visible = true,
                IconClass = "far fa-circle",
                SystemName = "SmartSlider"
            };
            menuItem.ChildNodes.Add(listItem);

            var configItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.SmartSliders.Menu.Configuration"),
                Url = "~/Admin/SmartSlider/Configure",
                Visible = true,
                IconClass = "far fa-circle",
                SystemName = "SmartSlider.Configuration"
            };
            menuItem.ChildNodes.Add(configItem);
        }

        if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
        {
            var documentation = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                Url = "https://www.nop-station.com/smart-slider-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=smart-slider",
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
        await _settingService.SaveSettingAsync(new SmartSliderSettings
        {
            EnableAjaxLoad = true,
            EnableSlider = true,
            SupportedVideoExtensions = new List<string> { "mp4", "avi", "mov", "flv" }
        });

        await this.InstallPluginAsync(new SmartSliderPermissionProvider());
        await base.InstallAsync();
    }

    public override async Task UninstallAsync()
    {
        await this.UninstallPluginAsync(new SmartSliderPermissionProvider());
        await base.UninstallAsync();
    }

    public List<KeyValuePair<string, string>> PluginResouces()
    {
        var list = new Dictionary<string, string>
        {
            ["Admin.NopStation.SmartSliders.Menu.SmartSlider"] = "Slider",
            ["Admin.NopStation.SmartSliders.Menu.Configuration"] = "Configuration",
            ["Admin.NopStation.SmartSliders.Menu.Sliders"] = "Sliders",

            ["Admin.NopStation.SmartSliders.Configuration"] = "Slider settings",
            ["Admin.NopStation.SmartSliders.Configuration.Fields.EnableSlider"] = "Enable slider",
            ["Admin.NopStation.SmartSliders.Configuration.Fields.EnableSlider.Hint"] = "Check to enable smart slider for your store.",
            ["Admin.NopStation.SmartSliders.Configuration.Fields.EnableAjaxLoad"] = "Enable AJAX load",
            ["Admin.NopStation.SmartSliders.Configuration.Fields.EnableAjaxLoad.Hint"] = "Determines whether sliders will load on AJAX.",
            ["Admin.NopStation.SmartSliders.Configuration.Fields.SupportedVideoExtensions"] = "Supported video extensions",
            ["Admin.NopStation.SmartSliders.Configuration.Fields.SupportedVideoExtensions.Hint"] = "Supported video extensions. (i.e. mp4, avi, mov, flv)",

            ["Admin.NopStation.SmartSliders.Sliders.List"] = "Sliders",
            ["Admin.NopStation.SmartSliders.Sliders.List.SearchActive.Active"] = "Active",
            ["Admin.NopStation.SmartSliders.Sliders.List.SearchActive.Inactive"] = "Inactive",
            ["Admin.NopStation.SmartSliders.Sliders.List.SearchKeyword"] = "Keyword",
            ["Admin.NopStation.SmartSliders.Sliders.List.SearchKeyword.Hint"] = "The search keyword.",
            ["Admin.NopStation.SmartSliders.Sliders.List.SearchWidgetZone"] = "Widget zone",
            ["Admin.NopStation.SmartSliders.Sliders.List.SearchWidgetZone.Hint"] = "The search widget zone.",
            ["Admin.NopStation.SmartSliders.Sliders.List.SearchStore"] = "Store",
            ["Admin.NopStation.SmartSliders.Sliders.List.SearchStore.Hint"] = "The search store.",
            ["Admin.NopStation.SmartSliders.Sliders.List.SearchActive"] = "Active",
            ["Admin.NopStation.SmartSliders.Sliders.List.SearchActive.Hint"] = "The search active.",

            ["Admin.NopStation.SmartSliders.Sliders.Tab.Info"] = "Info",
            ["Admin.NopStation.SmartSliders.Sliders.Tab.Properties"] = "Properties",
            ["Admin.NopStation.SmartSliders.Sliders.Tab.SliderItems"] = "Slider items",

            ["Admin.NopStation.SmartSliders.Sliders.EditDetails"] = "Edit slider details",
            ["Admin.NopStation.SmartSliders.Sliders.BackToList"] = "back to slider list",
            ["Admin.NopStation.SmartSliders.Sliders.AddNew"] = "Add new slider",

            ["Admin.NopStation.SmartSliders.Sliders.Items.AddNew"] = "Add new item",
            ["Admin.NopStation.SmartSliders.Sliders.Items.SaveBeforeEdit"] = "You need to save the slider before you can add items for this slider page.",
            ["Admin.NopStation.SmartSliders.Sliders.Items.EditDetails"] = "Edit item details",

            ["Admin.NopStation.SmartSliders.Sliders.Created"] = "Slider has been created successfully.",
            ["Admin.NopStation.SmartSliders.Sliders.Updated"] = "Slider has been updated successfully.",
            ["Admin.NopStation.SmartSliders.Sliders.Deleted"] = "Slider has been deleted successfully.",

            ["Admin.NopStation.SmartSliders.Sliders.Fields.Name"] = "Name",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.Name.Hint"] = "The slider name.",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.Name.Required"] = "The slider name is required.",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.Active"] = "Active",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.Active.Hint"] = "Determines whether this slider is active (visible on public store).",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.ShowBackground"] = "Show background",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.ShowBackground.Hint"] = "Determines whether to show background.",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.BackgroundType"] = "Background type",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.BackgroundType.Hint"] = "The slider background type.",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.BackgroundColor"] = "Background color",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.BackgroundColor.Hint"] = "The slider background color.",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.BackgroundPicture"] = "Background picture",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.BackgroundPicture.Hint"] = "The slider background picture.",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.BackgroundPicture.Required"] = "The background picture is required.",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.MaxProductsToShow"] = "Maximum products to show",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.MaxProductsToShow.Hint"] = "Specify the maximum number of products to show for this slider.",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.DisplayOrder"] = "Display order",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.DisplayOrder.Hint"] = "Display order of the slider. 1 represents the top of the list.",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.CreatedOn"] = "Created on",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.CreatedOn.Hint"] = "The create date of this slider.",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.UpdatedOn"] = "Updated on",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.UpdatedOn.Hint"] = "The last update date of this slider.",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.AclCustomerRoles"] = "Customer roles",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.AclCustomerRoles.Hint"] = "Select customer roles for which the slider will be shown. Leave empty if you want this slider to be visible to all users.",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.LimitedToStores"] = "Limited to stores",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.LimitedToStores.Hint"] = "Option to limit this slider to a certain store. If you have multiple stores, choose one or several from the list. If you don't use this option just leave this field empty.",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.CustomCssClass"] = "Custom CSS class",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.CustomCssClass.Hint"] = "Enter the custom CSS class to be applied.",

            ["Admin.NopStation.SmartSliders.Sliders.Fields.EnableAutoPlay"] = "Enable auto play",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.EnableAutoPlay.Hint"] = "Check to enable auto play.",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.AutoPlayTimeout"] = "Auto play timeout",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.AutoPlayTimeout.Hint"] = "It's autoplay interval timeout. (e.g 5000)",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.AutoPlayHoverPause"] = "Auto play hover pause",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.AutoPlayHoverPause.Hint"] = "Check to enable pause on mouse hover.",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.EnableLoop"] = "Enable loop",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.EnableLoop.Hint"] = "Check to enable loop.",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.StartPosition"] = "Start position",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.StartPosition.Hint"] = "The start position (e.g 0)",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.EnableKeyboardControl"] = "Enable keyboard control",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.EnableKeyboardControl.Hint"] = "Check to enable keyboard control.",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.KeyboardControlOnlyInViewport"] = "Only in viewport",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.KeyboardControlOnlyInViewport.Hint"] = "When enabled it will control sliders that are currently in viewport.",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.EnableNavigation"] = "Enable navigation",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.EnableNavigation.Hint"] = "Check to enable next/prev buttons.",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.EnableLazyLoad"] = "Enable lazy load",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.EnableLazyLoad.Hint"] = "Check to enable lazy load.",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.EnablePagination"] = "Enable pagination",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.EnablePagination.Hint"] = "Check to enable pagination.",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.PaginationType"] = "Pagination type",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.PaginationType.Hint"] = "Select pagination type.",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.PaginationDynamicMainBullets"] = "Dynamic main bullets",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.PaginationDynamicMainBullets.Hint"] = "The number of main bullets visible when dynamicBullets enabled.",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.PaginationDynamicBullets"] = "Dynamic bullets",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.PaginationDynamicBullets.Hint"] = "Good to enable if you use bullets pagination with a lot of slides. So it will keep only few bullets visible at the same time.",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.PaginationClickable"] = "Clickable",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.PaginationClickable.Hint"] = "If true then clicking on pagination button will cause transition to appropriate slide. Only for bullets pagination type.",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.EnableZoom"] = "Enable zoom",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.EnableZoom.Hint"] = "Check to enable zooming functionality.",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.ZoomMaximumRatio"] = "Maximum ratio",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.ZoomMaximumRatio.Hint"] = "Maximum image zoom multiplier.",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.ZoomMinimumRatio"] = "Minimum ratio",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.ZoomMinimumRatio.Hint"] = "Minimum image zoom multiplier.",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.ToggleZoom"] = "Toggle",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.ToggleZoom.Hint"] = "Enable/disable zoom-in by slide's double tap.",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.EnableEffect"] = "Enable effect",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.EnableEffect.Hint"] = "Check to enable effect.",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.EffectType"] = "Effect type",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.EffectType.Hint"] = "The slider effect type.",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.EnableMousewheelControl"] = "Enable mouse wheel control",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.EnableMousewheelControl.Hint"] = "Check to enable navigation through slides using mouse wheel.",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.MousewheelControlForceToAxis"] = "Force to axis",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.MousewheelControlForceToAxis.Hint"] = "Set to true to force mousewheel swipes to axis. So in horizontal mode mousewheel will work only with horizontal mousewheel scrolling, and only with vertical scrolling in vertical mode.",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.VerticalDirection"] = "Vertical direction",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.VerticalDirection.Hint"] = "Check to enable vertical direction. If unchecked slider direction will be horizontal.",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.AutoHeight"] = "Auto height",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.AutoHeight.Hint"] = "Set to true and slider wrapper will adapt its height to the height of the currently active slide.",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.AllowTouchMove"] = "Allow touch move",
            ["Admin.NopStation.SmartSliders.Sliders.Fields.AllowTouchMove.Hint"] = "If false, then the only way to switch the slide is use of external API functions like slidePrev or slideNext.",

            ["Admin.NopStation.SmartSliders.Sliders.Items.Fields.Content"] = "Content",
            ["Admin.NopStation.SmartSliders.Sliders.Items.Fields.Title"] = "Title",
            ["Admin.NopStation.SmartSliders.Sliders.Items.Fields.Title.Hint"] = "The slider item title.",
            ["Admin.NopStation.SmartSliders.Sliders.Items.Fields.Description"] = "Description",
            ["Admin.NopStation.SmartSliders.Sliders.Items.Fields.Description.Hint"] = "The slider item description.",
            ["Admin.NopStation.SmartSliders.Sliders.Items.Fields.RedirectUrl"] = "Redirect URL",
            ["Admin.NopStation.SmartSliders.Sliders.Items.Fields.RedirectUrl.Hint"] = "The redirect URL.",
            ["Admin.NopStation.SmartSliders.Sliders.Items.Fields.ButtonText"] = "ButtonText",
            ["Admin.NopStation.SmartSliders.Sliders.Items.Fields.ButtonText.Hint"] = "The button text.",
            ["Admin.NopStation.SmartSliders.Sliders.Items.Fields.DisplayOrder"] = "Display order",
            ["Admin.NopStation.SmartSliders.Sliders.Items.Fields.DisplayOrder.Hint"] = "Display order of the slider item. 1 represents the top of the list.",
            ["Admin.NopStation.SmartSliders.Sliders.Items.Fields.ShowCaption"] = "Show caption",
            ["Admin.NopStation.SmartSliders.Sliders.Items.Fields.ShowCaption.Hint"] = "Check to show caption.",
            ["Admin.NopStation.SmartSliders.Sliders.Items.Fields.ContentType"] = "Content type",
            ["Admin.NopStation.SmartSliders.Sliders.Items.Fields.ContentType.Hint"] = "The content type.",
            ["Admin.NopStation.SmartSliders.Sliders.Items.Fields.DesktopPictureId"] = "Desktop picture",
            ["Admin.NopStation.SmartSliders.Sliders.Items.Fields.DesktopPictureId.Hint"] = "The picture for desktop view.",
            ["Admin.NopStation.SmartSliders.Sliders.Items.Fields.MobilePictureId"] = "Mobile picture",
            ["Admin.NopStation.SmartSliders.Sliders.Items.Fields.MobilePictureId.Hint"] = "The picture for mobile view.",
            ["Admin.NopStation.SmartSliders.Sliders.Items.Fields.VideoDownloadId"] = "Video",
            ["Admin.NopStation.SmartSliders.Sliders.Items.Fields.VideoDownloadId.Hint"] = "Select video file.",
            ["Admin.NopStation.SmartSliders.Sliders.Items.Fields.EmbeddedLink"] = "Embedded link",
            ["Admin.NopStation.SmartSliders.Sliders.Items.Fields.EmbeddedLink.Hint"] = "The embedded link.",
            ["Admin.NopStation.SmartSliders.Sliders.Items.Fields.Text"] = "Text",
            ["Admin.NopStation.SmartSliders.Sliders.Items.Fields.Text.Hint"] = "The custom text (HTML).",
            ["Admin.NopStation.SmartSliders.Sliders.Items.Fields.Language"] = "Language",
            ["Admin.NopStation.SmartSliders.Sliders.Items.Fields.Language.Hint"] = "Option to limit this slider item to a certain language. Select 'All' to ignore this option.",

            ["NopStation.SmartSliders.LoadingFailed"] = "Failed to load carousel content.",
            ["NopStation.SmartSliders.ShopNow"] = "Shop Now",

            ["Admin.NopStation.SmartSliders.Sliders.Items.Fields.DesktopPictureId.Required"] = "The desktop picture is required.",
            ["Admin.NopStation.SmartSliders.Sliders.Items.Fields.MobilePictureId.Required"] = "The mobile picture is required.",
            ["Admin.NopStation.SmartSliders.Sliders.Items.Fields.EmbeddedLink.Required"] = "The embeddedlink is required.",
            ["Admin.NopStation.SmartSliders.Sliders.Items.Fields.Text.Required"] = "Text is required."

        };

        return list.ToList();
    }

    #endregion
}
