using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Services.Cms;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Widgets.Product360View.Components;

namespace NopStation.Plugin.Widgets.Product360View
{
    public class Product360ViewPlugin : BasePlugin, IWidgetPlugin, IAdminMenuPlugin, INopStationPlugin
    {
        #region Properties

        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly IPermissionService _permissionService;

        #endregion

        #region Ctors

        public Product360ViewPlugin(ILocalizationService localizationService,
            IWebHelper webHelper,
            INopStationCoreService nopStationCoreService,
            IPermissionService permissionService)
        {
            _localizationService = localizationService;
            _webHelper = webHelper;
            _permissionService = permissionService;
            _nopStationCoreService = nopStationCoreService;
        }

        #endregion

        #region Methods

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/WidgetsProduct360View/Configure";
        }

        public bool HideInWidgetList => false;

        public Type GetWidgetViewComponent(string widgetZone)
        {
            return typeof(Product360ViewComponent);
        }

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string> { AdminWidgetZones.ProductDetailsBlock, PublicWidgetZones.ProductDetailsAfterPictures });
        }

        public override async Task InstallAsync()
        {
            await this.InstallPluginAsync(new Product360ViewPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await this.UninstallPluginAsync(new Product360ViewPermissionProvider());
            await base.UninstallAsync();
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Plugins.Widgets.Product360View.Menu.Product360View", "Product 360 View"),
                new KeyValuePair<string, string>("Plugins.Widgets.Product360View.Menu.Configuration", "Configuration"),
                new KeyValuePair<string, string>("Plugins.Widgets.Product360View.IsEnabled", "Is enabled?"),
                new KeyValuePair<string, string>("Plugins.Widgets.Product360View.IsEnabled.Hint", "Determine is enabled or not."),
                new KeyValuePair<string, string>("Plugins.Widgets.Product360View.Fields.ProductId", "Product"),
                new KeyValuePair<string, string>("Plugins.Widgets.Product360View.Fields.DisplayOrder", "Display order"),
                new KeyValuePair<string, string>("Plugins.Widgets.Product360View.Fields.PictureId", "Picture"),
                new KeyValuePair<string, string>("Plugins.Widgets.Product360View.Fields.OverrideAltAttribute", "Alt"),
                new KeyValuePair<string, string>("Plugins.Widgets.Product360View.Fields.OverrideTitleAttribute", "Title"),
                new KeyValuePair<string, string>("Plugins.Widgets.Product360View.Fields.BehaviorTypeId", "Behavior type"),
                new KeyValuePair<string, string>("Plugins.Widgets.Product360View.Fields.BehaviorTypeId.Hint", "Specify the Behavior type. Select Mouse Drag, Mouse Movements or Mouse Wheel to move 360 image (animation will disabled in mouse wheel)"),
                new KeyValuePair<string, string>("Plugins.Widgets.Product360View.Fields.IsLoopEnabled", "Is loop enabled?"),
                new KeyValuePair<string, string>("Plugins.Widgets.Product360View.Fields.IsLoopEnabled.Hint", "Determine is loop enabled or not. It will be continuously spinning if it is enabled."),
                new KeyValuePair<string, string>("Plugins.Widgets.Product360View.Fields.IsZoomEnabled.Hint", "Determine is zoom enabled or not. Double click on 360 image will Show/Hide zoom view"),
                new KeyValuePair<string, string>("Plugins.Widgets.Product360View.Fields.IsZoomEnabled", "Is zoom enabled?"),
                new KeyValuePair<string, string>("Plugins.Widgets.Product360View.Fields.IsPanoramaEnabled", "Is panorama enabled?"),
                new KeyValuePair<string, string>("Plugins.Widgets.Product360View.Fields.IsPanoramaEnabled.Hint", "Click to enable Panorama View and upload a panorama image. Image with minimum display order will be selected if there are multiple images."),
                new KeyValuePair<string, string>("Plugins.Widgets.Product360View.360Picture", "360 Pictures"),
                new KeyValuePair<string, string>("Plugins.Widgets.Product360View.PanoramaPicture", "Panorama Pictures"),
            };
            return list;
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Plugins.Widgets.Product360View.Menu.Product360View"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(Product360ViewPermissionProvider.ManageProduct360View))
            {
                var configItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Plugins.Widgets.Product360View.Menu.Configuration"),
                    Url = $"{_webHelper.GetStoreLocation()}Admin/WidgetsProduct360View/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "WidgetsProduct360View.Configuration"
                };
                menuItem.ChildNodes.Add(configItem);
            }
            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/product360view-documentation?utm_source=admin-panel?utm_source=admin-panel&utm_medium=products&utm_campaign=product360view",
                    Visible = true,
                    IconClass = "far fa-circle",
                    OpenUrlInNewTab = true
                };
                menuItem.ChildNodes.Add(documentation);
            }

            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menuItem, NopStationMenuType.Plugin);
        }

        #endregion
    }
}
