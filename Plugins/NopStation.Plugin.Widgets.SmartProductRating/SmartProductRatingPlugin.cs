using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Widgets.SmartProductRating.Components;

namespace NopStation.Plugin.Widgets.SmartProductRating
{
    public class SmartProductRatingPlugin : BasePlugin, IWidgetPlugin, IAdminMenuPlugin, INopStationPlugin
    {
        private readonly SmartProductRatingSettings _ratingScorecardSettings;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;
        private readonly IWebHelper _webHelper;
        private readonly INopStationCoreService _nopStationCoreService;

        public SmartProductRatingPlugin(SmartProductRatingSettings ratingScorecardSettings,
            ILocalizationService localizationService,
            ISettingService settingService,
            IPermissionService permissionService,
            IWebHelper webHelper,
            INopStationCoreService nopStationCoreService)
        {
            _ratingScorecardSettings = ratingScorecardSettings;
            _localizationService = localizationService;
            _settingService = settingService;
            _permissionService = permissionService;
            _webHelper = webHelper;
            _nopStationCoreService = nopStationCoreService;
        }

        public override string GetConfigurationPageUrl()
        {
            return _webHelper.GetStoreLocation() + "Admin/SmartProductRating/Configure";
        }

        public override async Task InstallAsync()
        {
            _ratingScorecardSettings.ProductDetailsPageWidgetZone = "productdetails_before_collateral";
            _ratingScorecardSettings.NumberOfReviewsInProductDetailsPage = 10;
            await _settingService.SaveSettingAsync(_ratingScorecardSettings);

            await this.InstallPluginAsync(new SmartProductRatingPermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await this.UninstallPluginAsync(new SmartProductRatingPermissionProvider());
            await base.UninstallAsync();
        }

        public bool HideInWidgetList => false;

        public Type GetWidgetViewComponent(string widgetZone)
        {
            return typeof(SmartProductRatingViewComponent);
        }

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string>()
            {
                string.IsNullOrWhiteSpace(_ratingScorecardSettings.ProductDetailsPageWidgetZone) ?
                    "productdetails_before_collateral" : _ratingScorecardSettings.ProductDetailsPageWidgetZone
            });
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menu = new SiteMapNode()
            {
                Visible = true,
                IconClass = "far fa-dot-circle",
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.SmartProductRating.Menu.SmartProductRating")
            };

            if (await _permissionService.AuthorizeAsync(SmartProductRatingPermissionProvider.ManageConfiguration))
            {
                var configure = new SiteMapNode()
                {
                    Visible = true,
                    IconClass = "far fa-circle",
                    Url = "~/Admin/SmartProductRating/Configure",
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.SmartProductRating.Menu.Configuration"),
                    SystemName = "SmartProductRating.Configuration"
                };
                menu.ChildNodes.Add(configure);
            }

            var documentation = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                Url = "https://www.nop-station.com/smart-product-rating-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=smart-product-rating",
                Visible = true,
                IconClass = "far fa-circle",
                OpenUrlInNewTab = true
            };
            menu.ChildNodes.Add(documentation);

            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menu, NopStationMenuType.Plugin);
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Admin.NopStation.SmartProductRating.Menu.SmartProductRating", "Smart product rating"),
                new KeyValuePair<string, string>("Admin.NopStation.SmartProductRating.Menu.Configuration", "Configuration"),
                new KeyValuePair<string, string>("Admin.NopStation.SmartProductRating.Configuration", "Smart product rating settings"),

                new KeyValuePair<string, string>("Admin.NopStation.SmartProductRating.Configuration.Fields.EnablePlugin", "Enable plugin"),
                new KeyValuePair<string, string>("Admin.NopStation.SmartProductRating.Configuration.Fields.EnablePlugin.Hint", "Determines whether the plugin is enabled or not."),
                new KeyValuePair<string, string>("Admin.NopStation.SmartProductRating.Configuration.Fields.NumberOfReviewsInProductDetailsPage", "Number of reviews in product details page"),
                new KeyValuePair<string, string>("Admin.NopStation.SmartProductRating.Configuration.Fields.NumberOfReviewsInProductDetailsPage.Hint", "Enter page size of product reviews in product details page."),
                new KeyValuePair<string, string>("Admin.NopStation.SmartProductRating.Configuration.Fields.ProductDetailsPageWidgetZone", "Product details page widget zone"),
                new KeyValuePair<string, string>("Admin.NopStation.SmartProductRating.Configuration.Fields.ProductDetailsPageWidgetZone.Hint", "Enter widget zone of product details page where you want to display rating scorecard."),

                new KeyValuePair<string, string>("NopStation.SmartProductRating.OneStar", "One star"),
                new KeyValuePair<string, string>("NopStation.SmartProductRating.TwoStar", "Two star"),
                new KeyValuePair<string, string>("NopStation.SmartProductRating.ThreeStar", "Three star"),
                new KeyValuePair<string, string>("NopStation.SmartProductRating.FourStar", "Four star"),
                new KeyValuePair<string, string>("NopStation.SmartProductRating.FiveStar", "Five star"),
                new KeyValuePair<string, string>("NopStation.SmartProductRating.ReviewOf", "Ratings & Reviews of {0}"),
                new KeyValuePair<string, string>("NopStation.SmartProductRating.ProductReviews", "Product reviews"),
                new KeyValuePair<string, string>("NopStation.SmartProductRating.ShowAll", "show all"),
                new KeyValuePair<string, string>("NopStation.SmartProductRating.Ratings", "{0} Ratings"),
                new KeyValuePair<string, string>("NopStation.SmartProductRating.RatingsOutOf", "{0} out of 5 based on {1} reviews."),
                new KeyValuePair<string, string>("NopStation.SmartProductRating.NoReviews", "This product has no reviews. <br/>Let others know what do you think and be the first to write a review.")
            };

            return list;
        }
    }
}
