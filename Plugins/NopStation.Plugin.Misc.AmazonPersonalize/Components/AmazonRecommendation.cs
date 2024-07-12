using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Models.Catalog;
using NopStation.Plugin.Misc.AmazonPersonalize.Factories;
using NopStation.Plugin.Misc.AmazonPersonalize.Helpers;
using NopStation.Plugin.Misc.AmazonPersonalize.Services;
using NopStation.Plugin.Misc.Core.Components;

namespace NopStation.Plugin.Misc.AmazonPersonalize.Components
{
    public class AmazonRecommendationViewComponent : NopStationViewComponent
    {
        #region Fields

        private readonly AmazonPersonalizeSettings _amazonPersonalizeSettings;
        private readonly IRecommendationService _recommendationService;
        private readonly IPersonalizedRecommendationsModelFactory _personalizedRecommendationsModelFactory;

        #endregion Fields

        #region Ctor

        public AmazonRecommendationViewComponent(AmazonPersonalizeSettings amazonPersonalizeSettings,
            IRecommendationService recommendationService,
            IPersonalizedRecommendationsModelFactory personalizedRecommendationsModelFactory)
        {
            _amazonPersonalizeSettings = amazonPersonalizeSettings;
            _recommendationService = recommendationService;
            _personalizedRecommendationsModelFactory = personalizedRecommendationsModelFactory;
        }

        #endregion Ctor

        #region Methods

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            if (!_amazonPersonalizeSettings.EnableAmazonPersonalize)
                return Content("");

            if (!RecommendationHelper.TryGetWidgetZoneId(widgetZone, out int widgetZoneId))
                return Content("");

            var recommendations = (await _recommendationService.GetAllRecommendationAsync(new List<int> { widgetZoneId }, active: true));
            if (!recommendations.Any())
                return Content("");

            var productId = 0;
            if (additionalData != null && additionalData.GetType() == typeof(ProductDetailsModel))
            {
                var m = additionalData as ProductDetailsModel;
                productId = m.Id;
            }

            var model = await _personalizedRecommendationsModelFactory.PrepareRecommendationListModelAsync(recommendations, productId);

            return View(model);
        }

        #endregion Methods
    }
}