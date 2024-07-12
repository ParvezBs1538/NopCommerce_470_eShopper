using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Services.Localization;
using NopStation.Plugin.Misc.AmazonPersonalize.Domains;

namespace NopStation.Plugin.Misc.AmazonPersonalize.Services
{
    public class RecommendationService : IRecommendationService
    {
        #region Fields

        private readonly AmazonPersonalizeSettings _amazonPersonalizeSettings;
        private readonly ILocalizationService _localizationService;
        #endregion Fields



        #region Ctor

        public RecommendationService(AmazonPersonalizeSettings amazonPersonalizeSettings,
            ILocalizationService localizationService)
        {
            _amazonPersonalizeSettings = amazonPersonalizeSettings;
            _localizationService = localizationService;
        }

        #endregion Ctor

        #region Utilities
        public async Task<IList<Recommendation>> GetRecommendationsFromSettings()
        {
            var recommendations = new List<Recommendation>();

            var recommendedForYou = new Recommendation
            {
                Id = (int)RecommenderTypeEnum.RecommendedForYou,
                Active = _amazonPersonalizeSettings.EnableRecommendedForYou,
                RecommenderARN = _amazonPersonalizeSettings.RecommendedForYouARN,
                WidgetZoneId = _amazonPersonalizeSettings.RecommendedForYouWidgetZoneId,
                NumberOfItemsToShow = _amazonPersonalizeSettings.RecommendedForYouNumberOfItems,
                Title = await _localizationService.GetResourceAsync("NopStation.AmazonPersonalize.Recommendation.RecommendedForYou.Title"),
                AllowForGuestCustomer = _amazonPersonalizeSettings.RecommendedForYouAllowForGuestCustomer
            };
            recommendations.Add(recommendedForYou);

            var mostViewed = new Recommendation
            {
                Id = (int)RecommenderTypeEnum.MostViewed,
                Active = _amazonPersonalizeSettings.EnableMostViewed,
                RecommenderARN = _amazonPersonalizeSettings.MostViewedARN,
                WidgetZoneId = _amazonPersonalizeSettings.MostViewedWidgetZoneId,
                NumberOfItemsToShow = _amazonPersonalizeSettings.MostViewedNumberOfItems,
                Title = await _localizationService.GetResourceAsync("NopStation.AmazonPersonalize.Recommendation.MostViewed.Title"),
                AllowForGuestCustomer = _amazonPersonalizeSettings.MostViewedAllowForGuestCustomer
            };
            recommendations.Add(mostViewed);

            var customerWhoViewedXalsoViewed = new Recommendation
            {
                Id = (int)RecommenderTypeEnum.CustomersWhoViewedXAlsoViewed,
                Active = _amazonPersonalizeSettings.EnableCustomersWhoViewedXAlsoViewed,
                RecommenderARN = _amazonPersonalizeSettings.CustomersWhoViewedXAlsoViewedARN,
                WidgetZoneId = _amazonPersonalizeSettings.CustomersWhoViewedXAlsoViewedWidgetZoneId,
                NumberOfItemsToShow = _amazonPersonalizeSettings.CustomersWhoViewedXAlsoViewedNumberOfItems,
                Title = await _localizationService.GetResourceAsync("NopStation.AmazonPersonalize.Recommendation.CustomersWhoViewedXAlsoViewed.Title"),
                AllowForGuestCustomer = _amazonPersonalizeSettings.CustomersWhoViewedXAlsoViewedAllowForGuestCustomer
            };
            recommendations.Add(customerWhoViewedXalsoViewed);

            var bestSellers = new Recommendation
            {
                Id = (int)RecommenderTypeEnum.BestSellers,
                Active = _amazonPersonalizeSettings.EnableBestSellers,
                RecommenderARN = _amazonPersonalizeSettings.BestSellersARN,
                WidgetZoneId = _amazonPersonalizeSettings.BestSellersWidgetZoneId,
                NumberOfItemsToShow = _amazonPersonalizeSettings.BestSellersNumberOfItems,
                Title = await _localizationService.GetResourceAsync("NopStation.AmazonPersonalize.Recommendation.BestSellers.Title"),
                AllowForGuestCustomer = _amazonPersonalizeSettings.BestSellersAllowForGuestCustomer
            };
            recommendations.Add(bestSellers);

            var frequentlyBoughtTogether = new Recommendation
            {
                Id = (int)RecommenderTypeEnum.FrequentlyBoughtTogether,
                Active = _amazonPersonalizeSettings.EnableFrequentlyBoughtTogether,
                RecommenderARN = _amazonPersonalizeSettings.FrequentlyBoughtTogetherARN,
                WidgetZoneId = _amazonPersonalizeSettings.FrequentlyBoughtTogetherWidgetZoneId,
                NumberOfItemsToShow = _amazonPersonalizeSettings.FrequentlyBoughtTogetherNumberOfItems,
                Title = await _localizationService.GetResourceAsync("NopStation.AmazonPersonalize.Recommendation.FrequentlyBoughtTogether.Title"),
                AllowForGuestCustomer = _amazonPersonalizeSettings.FrequentlyBoughtTogetherAllowForGuestCustomer
            };
            recommendations.Add(frequentlyBoughtTogether);

            return recommendations;
        }
        #endregion

        #region Methods

        public virtual async Task<IList<Recommendation>> GetAllRecommendationAsync(List<int> widgetZoneIds, bool? active)
        {
            var recommendations = await GetRecommendationsFromSettings();

            var result = Enumerable.Empty<Recommendation>();

            if (widgetZoneIds != null && widgetZoneIds.Any())
                result = recommendations.Where(recommendation => widgetZoneIds.Contains(recommendation.WidgetZoneId));

            if (active.HasValue)
                result = result.Where(recommendation => recommendation.Active == active.Value);

            return result.ToList();
        }

        public virtual async Task<Recommendation> GetRecommendationByIdAsync(int recommendationId)
        {
            if (recommendationId == 0)
                return null;
            
            var recommendations = await GetRecommendationsFromSettings();
            return recommendations.Where(x => x.Id == recommendationId).FirstOrDefault();
        }

        #endregion Methods
    }
}