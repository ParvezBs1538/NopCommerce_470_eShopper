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
using NopStation.Plugin.Widgets.PrevNextProduct.Components;
using NopStation.Plugin.Widgets.PrevNextProduct.Domains;

namespace NopStation.Plugin.Widgets.PrevNextProduct;

public class PrevNextProductPlugin : BasePlugin, IWidgetPlugin, INopStationPlugin, IAdminMenuPlugin
{

    #region Fields

    private readonly ISettingService _settingService;
    private readonly PrevNextProductSettings _prevNextProductSettings;
    private readonly IPermissionService _permissionService;
    private readonly ILocalizationService _localizationService;
    private readonly INopStationCoreService _nopStationCoreService;
    private readonly IWebHelper _webHelper;

    public bool HideInWidgetList => false;

    #endregion

    #region Ctor

    public PrevNextProductPlugin(ISettingService settingService,
        PrevNextProductSettings prevNextProductSettings,
        IPermissionService permissionService,
        ILocalizationService localizationService,
        INopStationCoreService nopStationCoreService,
        IWebHelper webHelper)
    {
        _prevNextProductSettings = prevNextProductSettings;
        _permissionService = permissionService;
        _localizationService = localizationService;
        _nopStationCoreService = nopStationCoreService;
        _webHelper = webHelper;
        _settingService = settingService;
    }

    #endregion

    #region Methods

    public override string GetConfigurationPageUrl()
    {
        return $"{_webHelper.GetStoreLocation()}Admin/PrevNextProduct/Configure";
    }

    public override async Task InstallAsync()
    {
        await _settingService.SaveSettingAsync(new PrevNextProductSettings
        {
            WidgetZone = PublicWidgetZones.ProductDetailsTop,
            EnableLoop = true,
            NavigateBasedOnId = (int)NavigationType.Category,
            ProductNameMaxLength = 30
        });

        await this.InstallPluginAsync(new PrevNextProductPermissionProvider());
        await base.InstallAsync();
    }

    public override async Task UninstallAsync()
    {
        await this.UninstallPluginAsync(new PrevNextProductPermissionProvider());
        await base.UninstallAsync();
    }

    public List<KeyValuePair<string, string>> PluginResouces()
    {
        var list = new Dictionary<string, string>
        {
            ["Admin.NopStation.PrevNextProduct.Menu.PrevNextProduct"] = "Prev/Next product",
            ["Admin.NopStation.PrevNextProduct.Menu.Configuration"] = "Configuration",

            ["Admin.NopStation.PrevNextProduct.Configuration"] = "Prev/Next product settings",
            ["Admin.NopStation.PrevNextProduct.Configuration.Fields.EnableLoop"] = "Enable loop",
            ["Admin.NopStation.PrevNextProduct.Configuration.Fields.EnableLoop.Hint"] = "Check to enable loop. This will allow to show first product of specified catalog (i.e. Category, Manufacturer) page as 'Next product' when browing last product of that catalog. Also it will show last product as 'Previous product' when browsing the first product.",
            ["Admin.NopStation.PrevNextProduct.Configuration.Fields.WidgetZone"] = "Widget zone",
            ["Admin.NopStation.PrevNextProduct.Configuration.Fields.WidgetZone.Hint"] = "The widget zone of previous/next buttons in product details page.",
            ["Admin.NopStation.PrevNextProduct.Configuration.Fields.NavigateBasedOn"] = "Navigate based on",
            ["Admin.NopStation.PrevNextProduct.Configuration.Fields.NavigateBasedOn.Hint"] = "Navigate previous/next product based on catalog type.",
            ["Admin.NopStation.PrevNextProduct.Configuration.Fields.ProductNameMaxLength"] = "Product name max length",
            ["Admin.NopStation.PrevNextProduct.Configuration.Fields.ProductNameMaxLength.Hint"] = "The maximum length of product name to show in previous/next buttons.",
        };

        return list.ToList();
    }

    public Task<IList<string>> GetWidgetZonesAsync()
    {
        var widgetZone = string.IsNullOrWhiteSpace(_prevNextProductSettings.WidgetZone) ? PublicWidgetZones.ProductDetailsTop : _prevNextProductSettings.WidgetZone;

        return Task.FromResult<IList<string>>(new List<string> { widgetZone });
    }


    public Type GetWidgetViewComponent(string widgetZone)
    {
        return typeof(PrevNextProductViewComponent);
    }

    public async Task ManageSiteMapAsync(SiteMapNode rootNode)
    {
        if (await _permissionService.AuthorizeAsync(PrevNextProductPermissionProvider.ManageConfiguration))
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.PrevNextProduct.Menu.PrevNextProduct"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            var configItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.PrevNextProduct.Menu.Configuration"),
                Url = "~/Admin/PrevNextProduct/Configure",
                Visible = true,
                IconClass = "far fa-circle",
                SystemName = "PrevNextProduct.Configuration"
            };
            menuItem.ChildNodes.Add(configItem);

            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ManageConfiguration))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/previous-next-product-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=previous-next-product",
                    Visible = true,
                    IconClass = "far fa-circle",
                    OpenUrlInNewTab = true
                };
                menuItem.ChildNodes.Add(documentation);
            }

            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menuItem, NopStationMenuType.Plugin);
        }
    }


    #endregion
}
