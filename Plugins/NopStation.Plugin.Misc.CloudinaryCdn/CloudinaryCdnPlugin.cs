using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;

namespace NopStation.Plugin.Misc.CloudinaryCdn
{
    public class CloudinaryCdnPlugin : BasePlugin, INopStationPlugin, IAdminMenuPlugin, IMiscPlugin
    {
        #region Fields

        private readonly IWebHelper _webHelper;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;

        #endregion

        #region Ctor

        public CloudinaryCdnPlugin(IWebHelper webHelper,
            INopStationCoreService nopStationCoreService,
            ILocalizationService localizationService,
            IPermissionService permissionService)
        {
            _webHelper = webHelper;
            _nopStationCoreService = nopStationCoreService;
            _localizationService = localizationService;
            _permissionService = permissionService;
        }

        #endregion

        #region Methods

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/CloudinaryCdn/Configure";
        }

        public override async Task InstallAsync()
        {
            await this.InstallPluginAsync(new CloudinaryCdnPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await this.UninstallPluginAsync(new CloudinaryCdnPermissionProvider());
            await base.UninstallAsync();
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.CloudinaryCdn.Menu.CloudinaryCdn"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(CloudinaryCdnPermissionProvider.ManageConfiguration))
            {
                var configItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.CloudinaryCdn.Menu.Configuration"),
                    Url = $"{_webHelper.GetStoreLocation()}Admin/CloudinaryCdn/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "CloudinaryCdn.Configuration"
                };
                menuItem.ChildNodes.Add(configItem);
            }
            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/cloudinary-cdn-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=cloudinary-cdn",
                    Visible = true,
                    IconClass = "far fa-circle",
                    OpenUrlInNewTab = true
                };
                menuItem.ChildNodes.Add(documentation);
            }

            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menuItem, NopStationMenuType.Plugin);
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new Dictionary<string, string>
            {
                ["Admin.NopStation.CloudinaryCdn.Menu.Configuration"] = "Configuration",
                ["Admin.NopStation.CloudinaryCdn.Menu.CloudinaryCdn"] = "Cloudinary Cdn",

                ["Admin.NopStation.CloudinaryCdn.Configuration"] = "Cloudinary CDN settings",
                ["Admin.NopStation.CloudinaryCdn.Configuration.Fields.Enabled"] = "Enabled",
                ["Admin.NopStation.CloudinaryCdn.Configuration.Fields.Enabled.Hint"] = "Determines whether the plugin is enabled or not.",
                ["Admin.NopStation.CloudinaryCdn.Configuration.Fields.CloudName"] = "Cloud name",
                ["Admin.NopStation.CloudinaryCdn.Configuration.Fields.CloudName.Hint"] = "Define cloud name.",
                ["Admin.NopStation.CloudinaryCdn.Configuration.Fields.CloudName.Error"] = "Cloudinary Cloud Name is not specified.",
                ["Admin.NopStation.CloudinaryCdn.Configuration.Fields.ApiKey"] = "Api key",
                ["Admin.NopStation.CloudinaryCdn.Configuration.Fields.ApiKey.Hint"] = "Define Api key.",
                ["Admin.NopStation.CloudinaryCdn.Configuration.Fields.ApiKey.Error"] = "Cloudinary API key is not specified.",
                ["Admin.NopStation.CloudinaryCdn.Configuration.Fields.ApiSecret"] = "Api secret",
                ["Admin.NopStation.CloudinaryCdn.Configuration.Fields.ApiSecret.Hint"] = "Define Api secret.",
                ["Admin.NopStation.CloudinaryCdn.Configuration.Fields.ApiSecret.Error"] = "Cloudinary API Secret is not specified.",
                ["Admin.NopStation.CloudinaryCdn.Configuration.Fields.PrependCdnFolderName"] = "Prepend CDN folder name",
                ["Admin.NopStation.CloudinaryCdn.Configuration.Fields.PrependCdnFolderName.Hint"] = "Determines whether to prepend CDN folder name. WARNING: not recommended to change in production environment.",
                ["Admin.NopStation.CloudinaryCdn.Configuration.Fields.CdnFolderName"] = "CDN folder name",
                ["Admin.NopStation.CloudinaryCdn.Configuration.Fields.CdnFolderName.Hint"] = "Define CDN folder name. WARNING: not recommended to change in production environment.",
                ["Admin.NopStation.CloudinaryCdn.Configuration.Fields.EnableJsCdn"] = "Enable JS CDN",
                ["Admin.NopStation.CloudinaryCdn.Configuration.Fields.EnableJsCdn.Hint"] = "Determines whether to enable JS CDN.",
                ["Admin.NopStation.CloudinaryCdn.Configuration.Fields.EnableCssCdn"] = "Enable CSS CDN",
                ["Admin.NopStation.CloudinaryCdn.Configuration.Fields.EnableCssCdn.Hint"] = "Determines whether to enable CSS CDN.",
                ["Admin.NopStation.CloudinaryCdn.Configuration.Fields.AutoGenerateRequiredPictures"] = "Auto generatere quired pictures",
                ["Admin.NopStation.CloudinaryCdn.Configuration.Fields.AutoGenerateRequiredPictures.Hint"] = "Determines whether to auto generatere quired pictures (thumbnails).",
                ["Admin.NopStation.CloudinaryCdn.Configuration.Fields.AutoGenerateProductThumbPicture"] = "Auto generatere picture. Product thumb",
                ["Admin.NopStation.CloudinaryCdn.Configuration.Fields.AutoGenerateProductThumbPicture.Hint"] = "Auto generatere picture. Determines whether to generatere product thumb picture.",
                ["Admin.NopStation.CloudinaryCdn.Configuration.Fields.AutoGenerateProductDetailsPicture"] = "Auto generatere picture. Product details",
                ["Admin.NopStation.CloudinaryCdn.Configuration.Fields.AutoGenerateProductDetailsPicture.Hint"] = "Auto generatere picture. Determines whether to generatere product details picture.",
                ["Admin.NopStation.CloudinaryCdn.Configuration.Fields.AutoGenerateProductThumbPictureOnProductDetailsPage"] = "Auto generatere picture. Product thumb on product details page",
                ["Admin.NopStation.CloudinaryCdn.Configuration.Fields.AutoGenerateProductThumbPictureOnProductDetailsPage.Hint"] = "Auto generatere picture. Determines whether to generatere product thumb picture on product details page.",
                ["Admin.NopStation.CloudinaryCdn.Configuration.Fields.AutoGenerateAssociatedProductPicture"] = "Auto generatere picture. Associated product",
                ["Admin.NopStation.CloudinaryCdn.Configuration.Fields.AutoGenerateAssociatedProductPicture.Hint"] = "Auto generatere picture. Determines whether to generatere associated product picture.",
                ["Admin.NopStation.CloudinaryCdn.Configuration.Fields.AutoGenerateCategoryThumbPicture"] = "Auto generatere picture. Category thumb",
                ["Admin.NopStation.CloudinaryCdn.Configuration.Fields.AutoGenerateCategoryThumbPicture.Hint"] = "Auto generatere picture. Determines whether to generatere category thumb picture.",
                ["Admin.NopStation.CloudinaryCdn.Configuration.Fields.AutoGenerateManufacturerThumbPicture"] = "Auto generatere picture. Manufacturer thumb",
                ["Admin.NopStation.CloudinaryCdn.Configuration.Fields.AutoGenerateManufacturerThumbPicture.Hint"] = "Auto generatere picture. Determines whether to generatere manufacturer thumb picture.",
                ["Admin.NopStation.CloudinaryCdn.Configuration.Fields.AutoGenerateVendorThumbPicture"] = "Auto generatere picture. Vendor thumb",
                ["Admin.NopStation.CloudinaryCdn.Configuration.Fields.AutoGenerateVendorThumbPicture.Hint"] = "Auto generatere picture. Determines whether to generatere vendor thumb picture.",
                ["Admin.NopStation.CloudinaryCdn.Configuration.Fields.AutoGenerateCartThumbPicture"] = "Auto generatere picture. Cart thumb",
                ["Admin.NopStation.CloudinaryCdn.Configuration.Fields.AutoGenerateCartThumbPicture.Hint"] = "Auto generatere picture. Determines whether to generatere cart thumb picture.",
                ["Admin.NopStation.CloudinaryCdn.Configuration.Fields.AutoGenerateMiniCartThumbPicture"] = "Auto generatere picture. Mini cart thumb",
                ["Admin.NopStation.CloudinaryCdn.Configuration.Fields.AutoGenerateMiniCartThumbPicture.Hint"] = "Auto generatere picture. Determines whether to generatere mini cart thumb picture.",
                ["Admin.NopStation.CloudinaryCdn.Configuration.Fields.AutoGenerateAutoCompleteSearchThumbPicture"] = "Auto generatere picture. Auto complete search thumb",
                ["Admin.NopStation.CloudinaryCdn.Configuration.Fields.AutoGenerateAutoCompleteSearchThumbPicture.Hint"] = "Auto generatere picture. Determines whether to generatere auto complete search thumb picture."
            };

            return list.ToList();
        }

        #endregion
    }
}