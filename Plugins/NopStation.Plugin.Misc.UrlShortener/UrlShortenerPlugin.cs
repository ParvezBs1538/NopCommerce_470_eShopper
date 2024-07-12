using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;

namespace NopStation.Plugin.Misc.UrlShortener
{
    public class UrlShortenerPlugin : BasePlugin, INopStationPlugin, IAdminMenuPlugin
    {
        private readonly IWebHelper _webHelper;
        private readonly ILocalizationService _localizationService;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly IPermissionService _permissionService;

        public UrlShortenerPlugin(IWebHelper webHelper,
            ILocalizationService localizationService,
            INopStationCoreService nopStationCoreService,
            IPermissionService permissionService)
        {
            _webHelper = webHelper;
            _localizationService = localizationService;
            _nopStationCoreService = nopStationCoreService;
            _permissionService = permissionService;
        }

        public bool HideInWidgetList => false;

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menu = new SiteMapNode()
            {
                Visible = true,
                IconClass = "far fa-dot-circle",
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.UrlShortener.Menu")
            };

            if (await _permissionService.AuthorizeAsync(UrlShortnerPermissionProvider.ManageConfiguration))
            {
                var settings = new SiteMapNode()
                {
                    Visible = true,
                    IconClass = "far fa-circle",
                    Url = $"{_webHelper.GetStoreLocation()}Admin/UrlShortener/Configure",
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.UrlShortener.Menu.Configure"),
                    SystemName = "UrlShortener.Configuration"
                };
                menu.ChildNodes.Add(settings);
            }

            if (await _permissionService.AuthorizeAsync(UrlShortnerPermissionProvider.ManageUrlShortner))
            {
                var list = new SiteMapNode()
                {
                    IconClass = "far fa-circle",
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.UrlShortener.Menu.UrlList"),
                    Visible = true,
                    Url = $"{_webHelper.GetStoreLocation()}Admin/UrlShortener/List",
                    SystemName = "UrlShortener.ShortenUrlList"
                };
                menu.ChildNodes.Add(list);
            }

            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/bitly-url-shortener-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=bitly-url-shortener",
                    Visible = true,
                    IconClass = "far fa-circle",
                    OpenUrlInNewTab = true
                };
                menu.ChildNodes.Add(documentation);
            }
            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menu, NopStationMenuType.Plugin);
        }

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/UrlShortener/Configure";
        }

        public override async Task InstallAsync()
        {
            await this.InstallPluginAsync(new UrlShortnerPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await this.UninstallPluginAsync(new UrlShortnerPermissionProvider());
            await base.UninstallAsync();
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Admin.NopStation.UrlShortener.Configure.AccessToken", "Access token"),
                new KeyValuePair<string, string>("Admin.NopStation.UrlShortener.Configure.AccessToken.Hint", "Enter bit.ly access token"),
                new KeyValuePair<string, string>("Admin.NopStation.UrlShortener.Configure.EnableLog", "Enable log"),
                new KeyValuePair<string, string>("Admin.NopStation.UrlShortener.Configure.EnableLog.Hint", "Enable log"),

                new KeyValuePair<string, string>("Admin.NopStation.UrlShortener.ShortenUrlSearch.Slug", "Search engine friendly page name"),
                new KeyValuePair<string, string>("Admin.NopStation.UrlShortener.ShortenUrlSearch.Slug.Hint", "Search engine friendly page name"),

                new KeyValuePair<string, string>("Admin.NopStation.UrlShortener.ShortenUrlSearch.UrlEntityName", "Url type"),
                new KeyValuePair<string, string>("Admin.NopStation.UrlShortener.ShortenUrlSearch.UrlEntityName.Hint", "Search by url type"),

                new KeyValuePair<string, string>("Admin.NopStation.UrlShortener.ShortenUrl.UrlRecordId", "Url record"),
                new KeyValuePair<string, string>("Admin.NopStation.UrlShortener.ShortenUrl.UrlRecordId.Hint", "Url record"),
                new KeyValuePair<string, string>("Admin.NopStation.UrlShortener.ShortenUrl.Slug", "Search engine friendly page name"),
                new KeyValuePair<string, string>("Admin.NopStation.UrlShortener.ShortenUrl.Slug.Hint", "Search engine friendly page name"),
                new KeyValuePair<string, string>("Admin.NopStation.UrlShortener.ShortenUrl.ShortUrl", "Short url"),
                new KeyValuePair<string, string>("Admin.NopStation.UrlShortener.ShortenUrl.ShortUrl.Hint", "Short url which is generated by bit.ly"),
                new KeyValuePair<string, string>("Admin.NopStation.UrlShortener.ShortenUrl.Hash", "Hash"),
                new KeyValuePair<string, string>("Admin.NopStation.UrlShortener.ShortenUrl.Copy", "Copy"),
                new KeyValuePair<string, string>("Admin.NopStation.UrlShortener.ShortenUrl.Copy.NotFound", "No short url found to copy"),
                new KeyValuePair<string, string>("Admin.NopStation.UrlShortener.ShortenUrl.Hash.Hint", "Bit.ly provided hash for short url"),
                new KeyValuePair<string, string>("Admin.NopStation.UrlShortener.ShortenUrl.GlobalHash", "Global hash"),
                new KeyValuePair<string, string>("Admin.NopStation.UrlShortener.ShortenUrl.GlobalHash.Hint", "Bit.ly provided global hash for short url"),
                new KeyValuePair<string, string>("Admin.NopStation.UrlShortener.ShortenUrl.NewHash", "New hash"),
                new KeyValuePair<string, string>("Admin.NopStation.UrlShortener.ShortenUrl.NewHash.Hint", "Bit.ly provided new hash for short url"),

                new KeyValuePair<string, string>("Admin.NopStation.UrlShortener.Configure", "Configuration"),
                new KeyValuePair<string, string>("Admin.NopStation.UrlShortener.ShortenUrlList", "Shorten Url List"),
                new KeyValuePair<string, string>("Admin.NopStation.UrlShortener.AccessToken.IsNull", "Please add access token from configure page"),
                new KeyValuePair<string, string>("Admin.NopStation.UrlShortener.AccessToken.Invalid", "Invalid access token"),
                new KeyValuePair<string, string>("Admin.NopStation.UrlShortener.GenerateShortenUrl.Success", "Shorten urls generated successfully. {0} success, {1} fail."),
                new KeyValuePair<string, string>("Admin.NopStation.UrlShortener.GenerateShortenUrl.Warning", "Shorten urls generated successfully with warning. {0} success, {1} fail."),
                new KeyValuePair<string, string>("Admin.NopStation.UrlShortener.GenerateShortenUrl.Error", "Shorten urls generate failed. {0} success, {1} fail."),
                new KeyValuePair<string, string>("Admin.NopStation.UrlShortener.UpdateConfigure.Success", "Configuration updated successfully."),
                new KeyValuePair<string, string>("Admin.NopStation.UrlShortener.ShortenUrlGenerate", "Generate Shorten Url from Url Records"),
                new KeyValuePair<string, string>("Admin.NopStation.UrlShortener.Button.Generate", "Generate"),

                new KeyValuePair<string, string>("Admin.NopStation.UrlShortener.Button.GenerateSelected", "Generate selected"),
                new KeyValuePair<string, string>("Admin.NopStation.UrlShortener.Button.GenerateAll", "Generate all"),

                new KeyValuePair<string, string>("Admin.NopStation.UrlShortener.Menu", "Url shortener"),
                new KeyValuePair<string, string>("Admin.NopStation.UrlShortener.Menu.Configure", "Configuration"),
                new KeyValuePair<string, string>("Admin.NopStation.UrlShortener.Menu.UrlList", "Url list")
            };

            return list;
        }
    }
}
