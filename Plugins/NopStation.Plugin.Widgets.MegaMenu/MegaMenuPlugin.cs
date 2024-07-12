using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;

namespace NopStation.Plugin.Widgets.MegaMenu;

public class MegaMenuPlugin : BasePlugin, IAdminMenuPlugin, INopStationPlugin, IMiscPlugin
{
    #region Fields

    private readonly IWebHelper _webHelper;
    private readonly INopStationCoreService _nopStationCoreService;
    private readonly ILocalizationService _localizationService;
    private readonly IPermissionService _permissionService;
    private readonly INopFileProvider _fileProvider;
    private readonly IPictureService _pictureService;
    private readonly ISettingService _settingService;

    #endregion

    #region Ctor

    public MegaMenuPlugin(IWebHelper webHelper,
        INopStationCoreService nopStationCoreService,
        ILocalizationService localizationService,
        IPermissionService permissionService,
        INopFileProvider fileProvider,
        IPictureService pictureService,
        ISettingService settingService)
    {
        _webHelper = webHelper;
        _nopStationCoreService = nopStationCoreService;
        _localizationService = localizationService;
        _permissionService = permissionService;
        _fileProvider = fileProvider;
        _pictureService = pictureService;
        _settingService = settingService;
    }

    #endregion

    #region Methods

    public override string GetConfigurationPageUrl()
    {
        return _webHelper.GetStoreLocation() + "Admin/MegaMenu/Configure";
    }

    public override async Task InstallAsync()
    {
        var sampleImagesPath = _fileProvider.MapPath("~/Plugins/NopStation.Plugin.Widgets.MegaMenu/Install/");
        var samplePictureId = (await _pictureService.InsertPictureAsync(await _fileProvider.ReadAllBytesAsync(_fileProvider.Combine(sampleImagesPath, "nop-station.png")), MimeTypes.ImagePng, "nop-station")).Id;

        var settings = new MegaMenuSettings()
        {
            EnableMegaMenu = true,
            MaxCategoryLevelsToShow = 4,
            ShowNumberOfCategoryProducts = true,
            ShowNumberOfCategoryProductsIncludeSubcategories = true,
            DefaultCategoryIconId = samplePictureId
        };
        await _settingService.SaveSettingAsync(settings);

        await this.InstallPluginAsync(new MegaMenuPermissionProvider());
        await base.InstallAsync();
    }

    public override async Task UninstallAsync()
    {
        await this.UninstallPluginAsync(new MegaMenuPermissionProvider());
        await base.UninstallAsync();
    }

    public async Task ManageSiteMapAsync(SiteMapNode rootNode)
    {
        var menuItem = new SiteMapNode()
        {
            Title = await _localizationService.GetResourceAsync("Admin.NopStation.MegaMenu.Menu.MegaMenu"),
            Visible = true,
            IconClass = "far fa-dot-circle",
        };

        if (await _permissionService.AuthorizeAsync(MegaMenuPermissionProvider.ManageMegaMenu))
        {
            var categoryIcon = new SiteMapNode()
            {
                Visible = true,
                IconClass = "far fa-circle",
                Url = "/Admin/CategoryIcon/List",
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.MegaMenu.CategoryIcons"),
                SystemName = "MegaMenuCategoryIcon"
            };
            menuItem.ChildNodes.Add(categoryIcon);

            var configItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.MegaMenu.Menu.Configuration"),
                Url = "/Admin/MegaMenu/Configure",
                Visible = true,
                IconClass = "far fa-circle",
                SystemName = "MegaMenu.Configuration"
            };
            menuItem.ChildNodes.Add(configItem);
        }

        var documentation = new SiteMapNode()
        {
            Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
            Url = "https://www.nop-station.com/mega-menu-plugin",
            Visible = true,
            IconClass = "far fa-circle",
            OpenUrlInNewTab = true
        };
        menuItem.ChildNodes.Add(documentation);

        await _nopStationCoreService.ManageSiteMapAsync(rootNode, menuItem, NopStationMenuType.Plugin);
    }

    public List<KeyValuePair<string, string>> PluginResouces()
    {
        var list = new List<KeyValuePair<string, string>>();

        list.Add(new KeyValuePair<string, string>("Admin.NopStation.MegaMenu.Menu.MegaMenu", "Mega menu"));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.MegaMenu.Menu.Configuration", "Configuration"));

        list.Add(new KeyValuePair<string, string>("Admin.NopStation.MegaMenu.Configuration.Fields.ShowMainCategoryPictureRight", "Enable Left panel image"));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.MegaMenu.Configuration.Fields.ShowMainCategoryPictureRight.Hint", "Enable Left panel image"));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.MegaMenu.Configuration.Fields.EnableMegaMenu", "Enable mega menu"));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.MegaMenu.Configuration.Fields.EnableMegaMenu.Hint", "Check to enable mega menu. Restart application after changing value of this property."));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.MegaMenu.Configuration.Fields.MaxCategoryLevelsToShow", "Max category level"));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.MegaMenu.Configuration.Fields.MaxCategoryLevelsToShow.Hint", "Maximum category level to be displayed on top menu."));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.MegaMenu.Configuration.Fields.ShowNumberOfCategoryProducts", "Show number of category products"));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.MegaMenu.Configuration.Fields.ShowNumberOfCategoryProducts.Hint", "Determines whether number of category products to be displayed on top menu or not."));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.MegaMenu.Configuration.Fields.ShowNumberOfCategoryProductsIncludeSubcategories", "Include sub-category products"));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.MegaMenu.Configuration.Fields.ShowNumberOfCategoryProductsIncludeSubcategories.Hint", "Show category product number including sub-categories."));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.MegaMenu.Configuration.Fields.SelectedCategoryIds", "Selected categories"));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.MegaMenu.Configuration.Fields.SelectedCategoryIds.Hint", "Selected categories to be displayed on top menu."));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.MegaMenu.Configuration.Fields.ShowCategoryPicture", "Show category picture"));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.MegaMenu.Configuration.Fields.ShowCategoryPicture.Hint", "Show category picture on top menu."));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.MegaMenu.Configuration.Fields.DefaultCategoryIconId", "Default category icon"));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.MegaMenu.Configuration.Fields.DefaultCategoryIconId.Hint", "The default category icon to show on mega menu"));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.MegaMenu.Configuration.Fields.ShowSubcategoryPicture", "Show sub-category picture"));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.MegaMenu.Configuration.Fields.ShowSubcategoryPicture.Hint", "Show sub-category picture on top menu."));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.MegaMenu.Configuration.Fields.HideManufacturers", "Hide manufacturers"));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.MegaMenu.Configuration.Fields.HideManufacturers.Hint", "Hide manufacturers from top menu."));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.MegaMenu.Configuration.Fields.SelectedManufacturerIds", "Selected manufacturers"));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.MegaMenu.Configuration.Fields.SelectedManufacturerIds.Hint", "Selected manufacturers to be displayed on top menu."));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.MegaMenu.Configuration.Fields.ShowManufacturerPicture", "Show manufacturer picture"));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.MegaMenu.Configuration.Fields.ShowManufacturerPicture.Hint", "Show manufacturer picture on top menu."));

        list.Add(new KeyValuePair<string, string>("Admin.NopStation.MegaMenu.Configuration.Updated", "Mega menu configuration has been updated successfully."));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.MegaMenu.Configuration", "Mega menu settings"));

        list.Add(new KeyValuePair<string, string>("NopStation.MegaMenu.Public.Categories", "Categories"));
        list.Add(new KeyValuePair<string, string>("NopStation.MegaMenu.Public.Manufacturers", "Manufacturers"));
        list.Add(new KeyValuePair<string, string>("NopStation.MegaMenu.Public.AllManufacturers", "All Manufacturers"));

        list.Add(new KeyValuePair<string, string>("Admin.NopStation.MegaMenu.CategoryIcons", "Category Icons"));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.MegaMenu.CategoryIcons.Fields.Category", "Category"));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.MegaMenu.CategoryIcons.Fields.Picture", "Picture"));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.MegaMenu.CategoryIcons.List.SearchCategoryName", "Search Category Name"));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.MegaMenu.CategoryIcons.List.SearchStore", "Search Store"));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.MegaMenu.CategoryIcons.Fields.Picture.Required", "Picture is required"));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.MegaMenu.CategoryIcons.Fields.Category.Required", "Category is required"));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.MegaMenu.CategoryIcons.AddNew", "Add New"));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.MegaMenu.CategoryIcons.BackToList", "Back To List"));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.MegaMenu.CategoryIcons.EditDetails", "Edit Details"));
        list.Add(new KeyValuePair<string, string>("Admin.NopStation.MegaMenu.CategoryIcons.List", "List"));

        return list;
    }

    #endregion
}