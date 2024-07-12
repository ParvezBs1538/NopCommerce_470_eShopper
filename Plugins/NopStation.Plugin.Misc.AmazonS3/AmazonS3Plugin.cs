using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;

namespace NopStation.Plugin.Misc.AmazonS3
{
    public class AmazonS3Plugin : BasePlugin, IMiscPlugin, IAdminMenuPlugin, INopStationPlugin
    {
        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly IPermissionService _permissionService;

        public AmazonS3Plugin(ILocalizationService localizationService,
            IWebHelper webHelper,
            INopStationCoreService nopStationCoreService,
            IPermissionService permissionService)
        {
            _localizationService = localizationService;
            _webHelper = webHelper;
            _nopStationCoreService = nopStationCoreService;
            _permissionService = permissionService;
        }

        public override string GetConfigurationPageUrl()
        {
            return _webHelper.GetStoreLocation() + "Admin/AmazonS3/Configure";
        }

        public override async Task InstallAsync()
        {
            await this.InstallPluginAsync(new AmazonS3PermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await this.UninstallPluginAsync(new AmazonS3PermissionProvider());
            await base.UninstallAsync();
        }
        
        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Plugin.Misc.AmazonS3.Menu.AmazonS3"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(AmazonS3PermissionProvider.ManageConfiguration))
            {
                var configItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Plugin.Misc.AmazonS3.Menu.Configuration"),
                    Url = $"{_webHelper.GetStoreLocation()}Admin/AmazonS3/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "AmazonS3.Configuration"
                };
                menuItem.ChildNodes.Add(configItem);
            }

            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/amazon-s3-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=amazon-s3",
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
            var list = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AmazonS3.Configuration.Fields.AWSS3Enable", "Enable AWS S3"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AmazonS3.Configuration.Fields.RegionEndpoint", "Region"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AmazonS3.Configuration.Fields.AWSS3AccessKeyId", "AWS S3 access key id"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AmazonS3.Configuration.Fields.AWSS3SecretKey", "AWS S3 secret key"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AmazonS3.Configuration.Fields.AWSS3BucketName", "AWS S3 bucket name"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AmazonS3.Configuration.Fields.AWSS3RootUrl", "AWS S3 root url"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AmazonS3.Configuration.Fields.ExpiresDays", "Expires days"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AmazonS3.Configuration.Fields.EnableCdn", "Enable CDN"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AmazonS3.Configuration.Fields.CdnBaseUrl", "CDN base url"),

                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AmazonS3.Configuration.Fields.AWSS3Enable.Hint", "Enable aws s3."),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AmazonS3.Configuration.Fields.RegionEndpoint.Hint", "Select region."),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AmazonS3.Configuration.Fields.AWSS3AccessKeyId.Hint", "Enter aws s3 access key id."),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AmazonS3.Configuration.Fields.AWSS3SecretKey.Hint", "Enter aws s3 secret key."),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AmazonS3.Configuration.Fields.AWSS3BucketName.Hint", "Enter aws s3 bucket name."),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AmazonS3.Configuration.Fields.AWSS3RootUrl.Hint", "Enter aws s3 root url."),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AmazonS3.Configuration.Fields.CdnBaseUrl.Hint", "Enter CDN base url."),

                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AmazonS3.Configuration.Info", "After enable / disable option please restart the application."),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AmazonS3.Configuration", "Amazon S3 settings"),

                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AmazonS3.Menu.AmazonS3", "Amazon S3"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AmazonS3.Menu.Configuration", "Configuration"),

                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AmazonS3.Configuration.Fields.CannedACL", "Canned ACL"),
                new KeyValuePair<string, string>("Admin.NopStation.Plugin.Misc.AmazonS3.Configuration.Fields.CannedACL.Hint", "Select canned acl.")
            };

            return list;
        }
    }
}