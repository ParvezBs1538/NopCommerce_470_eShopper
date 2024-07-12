using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.PersonalizeRuntime;
using Amazon.PersonalizeRuntime.Model;
using Amazon.Runtime;
using Nop.Core;
using Nop.Services.Logging;
using NopStation.Plugin.Misc.AmazonPersonalize.Domains;
using NopStation.Plugin.Misc.AmazonPersonalize.Helpers;

namespace NopStation.Plugin.Misc.AmazonPersonalize.Services
{
    public class PersonalizedRecommendationsService : IPersonalizedRecommendationsService
    {
        #region Fields

        private readonly AmazonPersonalizeSettings _amazonPersonalizeSettings;
        private readonly ILogger _logger;
        private readonly IWorkContext _workContext;

        #endregion Fields

        #region Ctor

        public PersonalizedRecommendationsService(AmazonPersonalizeSettings amazonPersonalizeSettings,
            ILogger logger,
            IWorkContext workContext)
        {
            _amazonPersonalizeSettings = amazonPersonalizeSettings;
            _logger = logger;
            _workContext = workContext;
        }

        #endregion Ctor

        #region Utilities

        private AmazonPersonalizeRuntimeClient GetAmazonPersonalizeRuntimeClient()
        {
            return new AmazonPersonalizeRuntimeClient(new BasicAWSCredentials(_amazonPersonalizeSettings.AccessKey, _amazonPersonalizeSettings.SecretKey), AwsRegionHelper.GetRegionEndPoint(AwsRegionHelper.GetAwsRegion(_amazonPersonalizeSettings.AwsRegionId)));
        }

        #endregion Utilities

        #region Methods

        public virtual async Task<List<PredictedItem>> GetRecommendationResults(string userId, Recommendation recommendation, string itemId = null)
         {
            var recommendedItems = new List<PredictedItem>();

            try
            {
                var request = new GetRecommendationsRequest
                {
                    RecommenderArn = recommendation.RecommenderARN,
                    UserId = userId,
                    NumResults = recommendation.NumberOfItemsToShow,
                    ItemId = itemId,
                };

                var response = await GetAmazonPersonalizeRuntimeClient().GetRecommendationsAsync(request);
                recommendedItems = response.ItemList;
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync(ex.Message, ex, await _workContext.GetCurrentCustomerAsync());
            }

            return recommendedItems;
        }

        #endregion Methods
    }
}