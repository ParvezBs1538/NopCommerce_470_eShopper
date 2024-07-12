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
using NopStation.Plugin.Widgets.PictureZoom.Components;

namespace NopStation.Plugin.Widgets.PictureZoom;

public class PictureZoomPlugin : BasePlugin, IAdminMenuPlugin, INopStationPlugin, IWidgetPlugin
{
    #region Fields

    private readonly IWebHelper _webHelper;
    private readonly INopStationCoreService _nopStationCoreService;
    private readonly ILocalizationService _localizationService;
    private readonly IPermissionService _permissionService;
    private readonly ISettingService _settingService;

    public bool HideInWidgetList => false;

    #endregion

    #region Ctor

    public PictureZoomPlugin(IWebHelper webHelper,
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
        return _webHelper.GetStoreLocation() + "Admin/PictureZoom/Configure";
    }

    public override async Task InstallAsync()
    {
        var settings = new PictureZoomSettings()
        {
            EnablePictureZoom = true
        };
        await _settingService.SaveSettingAsync(settings);

        await this.InstallPluginAsync(new PictureZoomPermissionProvider());
        await base.InstallAsync();
    }

    public override async Task UninstallAsync()
    {
        await this.UninstallPluginAsync(new PictureZoomPermissionProvider());
        await base.UninstallAsync();
    }

    public async Task ManageSiteMapAsync(SiteMapNode rootNode)
    {
        var menuItem = new SiteMapNode()
        {
            Title = await _localizationService.GetResourceAsync("Admin.NopStation.PictureZoom.Menu.PictureZoom"),
            Visible = true,
            IconClass = "far fa-dot-circle",
        };

        if (await _permissionService.AuthorizeAsync(PictureZoomPermissionProvider.ManagePictureZoom))
        {
            var configItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.PictureZoom.Menu.Configuration"),
                Url = "~/Admin/PictureZoom/Configure",
                Visible = true,
                IconClass = "far fa-circle",
                SystemName = "PictureZoom.Configuration"
            };
            menuItem.ChildNodes.Add(configItem);
        }

        var documentation = new SiteMapNode()
        {
            Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
            Url = "https://www.nop-station.com/picture-zoom-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=picture-zoom",
            Visible = true,
            IconClass = "far fa-circle",
            OpenUrlInNewTab = true
        };
        menuItem.ChildNodes.Add(documentation);

        await _nopStationCoreService.ManageSiteMapAsync(rootNode, menuItem, NopStationMenuType.Plugin);
    }

    public List<KeyValuePair<string, string>> PluginResouces()
    {
        var list = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("Admin.NopStation.PictureZoom.Menu.PictureZoom", "Picture zoom"),
            new KeyValuePair<string, string>("Admin.NopStation.PictureZoom.Menu.Configuration", "Configuration"),
            new KeyValuePair<string, string>("Admin.NopStation.PictureZoom.Configuration.Fields.EnablePictureZoom", "Enable picture zoom"),
            new KeyValuePair<string, string>("Admin.NopStation.PictureZoom.Configuration.Fields.EnablePictureZoom.Hint", "Check to enable picture zoom."),
            new KeyValuePair<string, string>("Admin.NopStation.PictureZoom.Configuration.Fields.ZoomWidth", "Zoom width"),
            new KeyValuePair<string, string>("Admin.NopStation.PictureZoom.Configuration.Fields.ZoomWidth.Hint", "Picture zoom width ratio according to picture."),
            new KeyValuePair<string, string>("Admin.NopStation.PictureZoom.Configuration.Fields.ZoomHeight", "Zoom height"),
            new KeyValuePair<string, string>("Admin.NopStation.PictureZoom.Configuration.Fields.ZoomHeight.Hint", "Picture width width ratio according to picture."),
            new KeyValuePair<string, string>("Admin.NopStation.PictureZoom.Configuration.Fields.LtrPositionTypeId", "Ltr position type"),
            new KeyValuePair<string, string>("Admin.NopStation.PictureZoom.Configuration.Fields.LtrPositionTypeId.Hint", "Picture zoom postion for left-to-right language (eg. top, right, inside. default: right)"),
            new KeyValuePair<string, string>("Admin.NopStation.PictureZoom.Configuration.Fields.RtlPositionTypeId", "Rtl position type"),
            new KeyValuePair<string, string>("Admin.NopStation.PictureZoom.Configuration.Fields.RtlPositionTypeId.Hint", "Picture zoom postion for right-to-left language (eg. top, right, inside. default: left)"),
            new KeyValuePair<string, string>("Admin.NopStation.PictureZoom.Configuration.Fields.Tint", "Tint"),
            new KeyValuePair<string, string>("Admin.NopStation.PictureZoom.Configuration.Fields.Tint.Hint", "Tint. (e.g false)"),
            new KeyValuePair<string, string>("Admin.NopStation.PictureZoom.Configuration.Fields.TintOpacity", "Tint opacity"),
            new KeyValuePair<string, string>("Admin.NopStation.PictureZoom.Configuration.Fields.TintOpacity.Hint", "Tint Opacity (e.g 0.5)"),
            new KeyValuePair<string, string>("Admin.NopStation.PictureZoom.Configuration.Fields.LensOpacity", "Lens opacity"),
            new KeyValuePair<string, string>("Admin.NopStation.PictureZoom.Configuration.Fields.LensOpacity.Hint", "Lens Opacity (e.g 0.5)"),
            new KeyValuePair<string, string>("Admin.NopStation.PictureZoom.Configuration.Fields.SoftFocus", "Soft focus"),
            new KeyValuePair<string, string>("Admin.NopStation.PictureZoom.Configuration.Fields.SoftFocus.Hint", "Soft Focus (e.g false)"),
            new KeyValuePair<string, string>("Admin.NopStation.PictureZoom.Configuration.Fields.SmoothMove", "Smooth move"),
            new KeyValuePair<string, string>("Admin.NopStation.PictureZoom.Configuration.Fields.SmoothMove.Hint", "Smooth Move (e.g 3)"),
            new KeyValuePair<string, string>("Admin.NopStation.PictureZoom.Configuration.Fields.ShowTitle", "Show title"),
            new KeyValuePair<string, string>("Admin.NopStation.PictureZoom.Configuration.Fields.ShowTitle.Hint", "Show Title (e.g true)"),
            new KeyValuePair<string, string>("Admin.NopStation.PictureZoom.Configuration.Fields.TitleOpacity", "Title opacity"),
            new KeyValuePair<string, string>("Admin.NopStation.PictureZoom.Configuration.Fields.TitleOpacity.Hint", "Title Opacity (e.g 0.5)"),
            new KeyValuePair<string, string>("Admin.NopStation.PictureZoom.Configuration.Fields.AdjustX", "Adjust X"),
            new KeyValuePair<string, string>("Admin.NopStation.PictureZoom.Configuration.Fields.AdjustX.Hint", "AdjustX (e.g 0)"),
            new KeyValuePair<string, string>("Admin.NopStation.PictureZoom.Configuration.Fields.AdjustY", "Adjust Y"),
            new KeyValuePair<string, string>("Admin.NopStation.PictureZoom.Configuration.Fields.AdjustY.Hint", "AdjustY (e.g 0)"),
            new KeyValuePair<string, string>("Admin.NopStation.PictureZoom.Configuration.Fields.ImageSize", "Image size"),
            new KeyValuePair<string, string>("Admin.NopStation.PictureZoom.Configuration.Fields.ImageSize.Hint", "Image Size (e.g 500)"),
            new KeyValuePair<string, string>("Admin.NopStation.PictureZoom.Configuration.Fields.FullSizeImage", "Full size image"),
            new KeyValuePair<string, string>("Admin.NopStation.PictureZoom.Configuration.Fields.FullSizeImage.Hint", "Full Size Image (e.g 1000)"),
            new KeyValuePair<string, string>("Admin.NopStation.PictureZoom.Configuration", "Picture zoom settings")
        };

        return list;
    }

    public Task<IList<string>> GetWidgetZonesAsync()
    {
        return Task.FromResult<IList<string>>(new List<string> { PublicWidgetZones.Footer });
    }

    public Type GetWidgetViewComponent(string widgetZone)
    {
        return typeof(PictureZoomViewComponent);
    }

    #endregion
}