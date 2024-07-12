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
using NopStation.Plugin.Misc.AmazonPersonalize.Components;
using NopStation.Plugin.Misc.AmazonPersonalize.Domains;
using NopStation.Plugin.Misc.AmazonPersonalize.Helpers;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;

namespace NopStation.Plugin.Misc.AmazonPersonalize
{
    public class AmazonPersonalizePlugin : BasePlugin, IWidgetPlugin, IAdminMenuPlugin, INopStationPlugin
    {
        #region Fields

        private readonly IWebHelper _webHelper;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly INopStationCoreService _nopStationCoreService;

        public bool HideInWidgetList => false;

        #endregion Fields

        #region Ctor

        public AmazonPersonalizePlugin(IWebHelper webHelper,
            ILocalizationService localizationService,
            IPermissionService permissionService,
            INopStationCoreService nopStationCoreService)
        {
            _webHelper = webHelper;
            _localizationService = localizationService;
            _permissionService = permissionService;
            _nopStationCoreService = nopStationCoreService;
        }

        #endregion Ctor

        #region Methods

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/AmazonPersonalize/Configure";
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menu = new SiteMapNode()
            {
                Visible = true,
                IconClass = "far fa-dot-circle",
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.AmazonPersonalize.Menu.AmazonPersonalize")
            };
            if (await _permissionService.AuthorizeAsync(AmazonPersonalizePermissionProvider.ManageConfiguration))
            {
                var configure = new SiteMapNode()
                {
                    Visible = true,
                    IconClass = "far fa-circle",
                    Url = "~/Admin/AmazonPersonalize/Configure",
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.AmazonPersonalize.Menu.Configuration"),
                    SystemName = "AmazonPersonalize.Configuration"
                };
                menu.ChildNodes.Add(configure);
            }

            if (await _permissionService.AuthorizeAsync(AmazonPersonalizePermissionProvider.ManageRecommenders))
            {
                var configure = new SiteMapNode()
                {
                    Visible = true,
                    IconClass = "far fa-circle",
                    Url = "~/Admin/AmazonPersonalize/RecommenderList",
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.AmazonPersonalize.Menu.Recommenders"),
                    SystemName = "AmazonPersonalize.Recommenders"
                };
                menu.ChildNodes.Add(configure);
            }

            if (await _permissionService.AuthorizeAsync(AmazonPersonalizePermissionProvider.ManageUploadInteractions))
            {
                var configure = new SiteMapNode()
                {
                    Visible = true,
                    IconClass = "far fa-circle",
                    Url = "~/Admin/AmazonPersonalize/UploadInteractionsData",
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.AmazonPersonalize.Menu.UploadInteractionsData"),
                    SystemName = "AmazonPersonalize.UploadInteractionsData"
                };
                menu.ChildNodes.Add(configure);
            }

            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.AmazonPersonalize.Menu.Documentation"),
                    Url = "https://www.nop-station.com/amazon-personalize-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=amazon-personalize",
                    Visible = true,
                    IconClass = "far fa-circle",
                    OpenUrlInNewTab = true
                };
                menu.ChildNodes.Add(documentation);
            }

            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menu, NopStationMenuType.Plugin);
        }

        public override async Task InstallAsync()
        {
            await this.InstallPluginAsync(new AmazonPersonalizePermissionProvider());
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await this.UninstallPluginAsync(new AmazonPersonalizePermissionProvider());
            await base.UninstallAsync();
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>
            {
                //TODO: some resource is in public change the key. 
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Menu.AmazonPersonalize", "Amazon Personalize"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Menu.Configuration", "Configuration"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Menu.Documentation", "Documentation"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Menu.Recommendations", "Recommendations"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.Fields.EnableAmazonPersonalize", "Enable amazon personalize"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.Fields.EnableAmazonPersonalize.Hint", "Check to enable amazon personalize for your store."),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.Fields.AccessKey", "Access key"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.Fields.AccessKey.Hint", "AWS access key."),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.Fields.SecretKey", "Secret key"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.Fields.SecretKey.Hint", "AWS secret key."),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.Fields.AwsRegion", "Aws region"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.Fields.AwsRegion.Hint", "Select Aws Region. Aws personalize may not be avilable in all regions."),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.Fields.EventTrackerId", "Event tracker id"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.Fields.EventTrackerId.Hint", "Event tracker id."),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration", "Configuration"),

                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.Fields.EnableRecommendedForYou", "Enable recommended for you recommender"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.Fields.EnableRecommendedForYou.Hint", "Check to enable recommended for you recommender."),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.Fields.RecommendedForYouARN", "Recommended for you ARN"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.Fields.RecommendedForYouARN.Hint", "Recommended for you ARN."),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.Fields.RecommendedForYouWidgetZoneId", "Widget zone"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.Fields.RecommendedForYouWidgetZoneId.Hint", "Select widget zone for recommended for you."),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.Fields.RecommendedForYouNumberOfItems", "Number of items"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.Fields.RecommendedForYouNumberOfItems.Hint", "Specify the number of items to show for this recommender."),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.Fields.RecommendedForYouAllowForGuestCustomer", "Allow for guest customer"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.Fields.RecommendedForYouAllowForGuestCustomer.Hint", "If you want to show this recommender to guest customer enable it."),

                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.Fields.EnableMostViewed", "Enable most viewed recommender"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.Fields.EnableMostViewed.Hint", "Check to enable most viewed recommender."),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.Fields.MostViewedARN", "Most viewed ARN"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.Fields.MostViewedARN.Hint", "Most viewed ARN."),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.Fields.MostViewedWidgetZoneId", "Widget zone"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.Fields.MostViewedWidgetZoneId.Hint", "Select widget zone for most viewed recommender."),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.Fields.MostViewedNumberOfItems", "Number of items"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.Fields.MostViewedNumberOfItems.Hint", "Specify the number of items to show for this recommender."),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.Fields.MostViewedAllowForGuestCustomer", "Allow for guest customer"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.Fields.MostViewedAllowForGuestCustomer.Hint", "If you want to show this recommender to guest customer enable it."),

                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.Fields.EnableCustomersWhoViewedXAlsoViewed", "Enable customer who viewed x also viewed recommender"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.Fields.EnableCustomersWhoViewedXAlsoViewed.Hint", "Check to enable customer who viewed x also viewed recommender."),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.Fields.CustomersWhoViewedXAlsoViewedARN", "Customer who viewed x also viewed ARN"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.Fields.CustomersWhoViewedXAlsoViewedARN.Hint", "Customer who viewed x also viewed ARN."),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.Fields.CustomersWhoViewedXAlsoViewedWidgetZoneId", "Widget zone"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.Fields.CustomersWhoViewedXAlsoViewedWidgetZoneId.Hint", "Select widget zone for Customer who viewed x also viewed recommender."),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.Fields.CustomersWhoViewedXAlsoViewedNumberOfItems", "Number of items"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.Fields.CustomersWhoViewedXAlsoViewedNumberOfItems.Hint", "Specify the number of items to show for this recommender."),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.Fields.CustomersWhoViewedXAlsoViewedAllowForGuestCustomer", "Allow for guest customer"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.Fields.CustomersWhoViewedXAlsoViewedAllowForGuestCustomer.Hint", "If you want to show this recommender to guest customer enable it."),

                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.Fields.EnableBestSellers", "Enable bestsellers recommender"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.Fields.EnableBestSellers.Hint", "Check to enable bestsellers recommender."),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.Fields.BestSellersARN", "Best sellers ARN"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.Fields.BestSellersARN.Hint", "Best sellers ARN."),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.Fields.BestSellersWidgetZoneId", "Widget zone"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.Fields.BestSellersWidgetZoneId.Hint", "Select widget zone for bestsellers recommender."),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.Fields.BestSellersNumberOfItems", "Number of items"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.Fields.BestSellersNumberOfItems.Hint", "Specify the number of items to show for this recommender."),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.Fields.BestSellersAllowForGuestCustomer", "Allow for guest customer"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.Fields.BestSellersAllowForGuestCustomer.Hint", "If you want to show this recommender to guest customer enable it."),

                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.Fields.EnableFrequentlyBoughtTogether", "Enable frequently bought together"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.Fields.EnableFrequentlyBoughtTogether.Hint", "Check to enable frequently bought together recommender."),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.Fields.FrequentlyBoughtTogetherARN", "Frequently bought together ARN"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.Fields.FrequentlyBoughtTogetherARN.Hint", "Frequently bought together ARN."),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.Fields.FrequentlyBoughtTogetherWidgetZoneId", "Widget zone"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.Fields.FrequentlyBoughtTogetherWidgetZoneId.Hint", "Select widget zone for frequently bought together recommender."),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.Fields.FrequentlyBoughtTogetherNumberOfItems", "Number of items"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.Fields.FrequentlyBoughtTogetherNumberOfItems.Hint", "Specify the number of items to show for this recommender."),
                 new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.Fields.FrequentlyBoughtTogetherAllowForGuestCustomer", "Allow for guest customer"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.Fields.FrequentlyBoughtTogetherAllowForGuestCustomer.Hint", "If you want to show this recommender to guest customer enable it."),

                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Recommender.RecommenderList", "Recommender list"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Recommender.Fields.Name", "Name"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Recommender.Fields.DatasetGroupArn", "Data set group ARN"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Recommender.Fields.RecommenderArn", "Recommender ARN"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Recommender.Fields.Status", "Status"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Recommender.Fields.RecipeArn", "Recipe ARN"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Recommender.Fields.CreationDateTime", "Creation date-time utc"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Menu.Recommenders", "Manage recommenders"),

                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Recommender.Fields.StartOrStop", "Start or Stop"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Recommender.Stop", "Stop"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Recommender.Start", "Start"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Recommender.Operation.Message", "An operation is in progress. Please wait..."),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Recommender.Alert.Error", "An error occured!"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.Fields.DataSetGroupArn", "Data set group ARN"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.Fields.DataSetGroupArn.Hint", "Put your Data set group ARN"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.BlockTitle.Common", "Common"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.BlockTitle.RecommendedForYou", "Recommended for you"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.BlockTitle.MostViewed", "Most viewed"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.BlockTitle.CustomersWhoViewedXAlsoViewed", "Customer who viewed x also viewed"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.BlockTitle.BestSellers", "Best sellers"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.BlockTitle.FrequentlyBoughtTogether", "Frequently bought together"),
                new KeyValuePair<string, string>("NopStation.AmazonPersonalize.Interactions.Fields.UserId", "USER_ID"),
                new KeyValuePair<string, string>("NopStation.AmazonPersonalize.Interactions.Fields.ItemId", "ITEM_ID"),
                new KeyValuePair<string, string>("NopStation.AmazonPersonalize.Interactions.Fields.EventType", "EVENT_TYPE"),
                new KeyValuePair<string, string>("NopStation.AmazonPersonalize.Interactions.Fields.TimeStamp", "TIMESTAMP"),
                new KeyValuePair<string, string>("NopStation.AmazonPersonalize.Customer.Message", "You need to login to view this content."),
                new KeyValuePair<string, string>("NopStation.AmazonPersonalize.Recommender.NoProducts", "Currently no recommended products for you."),
                new KeyValuePair<string, string>("NopStation.AmazonPersonalize.Recommendation.RecommendedForYou.Title", "Recommended for you"),
                new KeyValuePair<string, string>("NopStation.AmazonPersonalize.Recommendation.MostViewed.Title", "Most viewed"),
                new KeyValuePair<string, string>("NopStation.AmazonPersonalize.Recommendation.CustomersWhoViewedXAlsoViewed.Title", "Customer who viewed this also viewed"),
                new KeyValuePair<string, string>("NopStation.AmazonPersonalize.Recommendation.BestSellers.Title", "Best sellers"),
                new KeyValuePair<string, string>("NopStation.AmazonPersonalize.Recommendation.FrequentlyBoughtTogether.Title", "Frequently bought together"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Interactions.ExportToCsv", "Export interactions data to csv"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Interactions.Info", "Not enough data to export."),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Interactions.Error", "An error occured."),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.UploadInteractionsData.Title", "Upload interactions data"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.UploadInteractionsData.Upload", "Upload"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.UploadInteractionsData.StartDate", "Start date"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.UploadInteractionsData.StartDate.Hint", "The start date of order history data."),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.UploadInteractionsData.EndDate", "End date"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.UploadInteractionsData.EndDate.Hint", "The end date of order history data."),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Menu.UploadInteractionsData", "Upload interactions data"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.Fields.EnableLogging", "Enable logging"),
                new KeyValuePair<string, string>("Admin.NopStation.AmazonPersonalize.Configuration.Fields.EnableLogging.Hint", "Check to enable logging for events data while sending to aws personalize."),
            };

            return list;
        }

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            var widgetZones = RecommendationHelper.GetCustomWidgetZones();
            widgetZones.Add(PublicWidgetZones.Footer);
            return Task.FromResult<IList<string>>(widgetZones);
        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            if (widgetZone == PublicWidgetZones.Footer)
                return typeof(AmazonPersonalizeFooterViewComponent);

            return typeof(AmazonRecommendationViewComponent);
        }

        #endregion Methods
    }
}