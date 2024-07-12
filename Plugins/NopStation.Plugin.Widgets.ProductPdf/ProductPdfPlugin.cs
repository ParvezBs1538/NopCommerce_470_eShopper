using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;
using System;
using NopStation.Plugin.Widgets.ProductPdf.Components;

namespace NopStation.Plugin.Widgets.ProductPdf
{
    public class ProductPdfPlugin : BasePlugin, IWidgetPlugin, INopStationPlugin, IAdminMenuPlugin
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;
        private readonly INopStationCoreService _nopStationCoreService;

        #endregion

        #region Ctor

        public ProductPdfPlugin(ILocalizationService localizationService,
            IWebHelper webHelper,
            ISettingService settingService,
            IPermissionService permissionService,
            INopStationCoreService nopStationCoreService)
        {
            _localizationService = localizationService;
            _webHelper = webHelper;
            _settingService = settingService;
            _permissionService = permissionService;
            _nopStationCoreService = nopStationCoreService;
        }

        #endregion

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string>
            {
                PublicWidgetZones.ProductDetailsInsideOverviewButtonsAfter
            });
        }

        public override string GetConfigurationPageUrl()
        {
            return _webHelper.GetStoreLocation() + "Admin/ProductPdf/Configure";
        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            return typeof(ProductPdfViewComponent);
        }

        public bool HideInWidgetList => false;
        
        public override async Task InstallAsync()
        {
            //settings
            await _settingService.SaveSettingAsync(new ProductPdfSettings
            {
                EnablePlugin = true
            });

            await this.InstallPluginAsync(new ProductPdfPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            //settings
            await _settingService.DeleteSettingAsync<ProductPdfSettings>();

            await this.UninstallPluginAsync(new ProductPdfPermissionProvider());
            await base.UninstallAsync();
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            return new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Admin.NopStation.ProductPdf.Configuration.Fields.EnablePlugin", "Enable plugin"),
                new KeyValuePair<string, string>("Admin.NopStation.ProductPdf.Configuration.Fields.EnablePlugin.Hint", "Determines whether the plugin is enabled or not."),
                new KeyValuePair<string, string>("Admin.NopStation.ProductPdf.Configuration.Fields.LetterPageSizeEnabled", "Letter page size enabled"),
                new KeyValuePair<string, string>("Admin.NopStation.ProductPdf.Configuration.Fields.LetterPageSizeEnabled.Hint", "Determines whether letter page size enabled or not."),

                new KeyValuePair<string, string>("Admin.NopStation.ProductPdf.Configuration", "Product pdf settings"),
                new KeyValuePair<string, string>("Admin.NopStation.ProductPdf.Menu.Configuration", "Configuration"),
                new KeyValuePair<string, string>("Admin.NopStation.ProductPdf.Menu.ProductPdf", "Product pdf"),

                new KeyValuePair<string, string>("NopStation.ProductPdf.PrintPdf", "Print PDF"),
                new KeyValuePair<string, string>("NopStation.ProductPdf.Overview", "Overview"),
                new KeyValuePair<string, string>("NopStation.ProductPdf.Specifications", "Specifications"),
            };
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.ProductPdf.Menu.ProductPdf"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(ProductPdfPermissionProvider.ManageConfiguration))
            {
                var conItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.ProductPdf.Menu.Configuration"),
                    Url = "~/Admin/ProductPdf/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "ProductPdf.Configuration"
                };
                menuItem.ChildNodes.Add(conItem);
            }

            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/productpdf-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=productpdf",
                    Visible = true,
                    IconClass = "far fa-circle",
                    OpenUrlInNewTab = true
                };
                menuItem.ChildNodes.Add(documentation);
            }

            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menuItem, NopStationMenuType.Plugin);
        }
    }
}
