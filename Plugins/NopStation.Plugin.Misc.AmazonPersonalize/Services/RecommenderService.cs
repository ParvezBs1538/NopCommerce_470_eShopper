using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.Personalize;
using Amazon.Personalize.Model;
using Nop.Core;
using Nop.Services.Logging;
using NopStation.Plugin.Misc.AmazonPersonalize.Areas.Admin.Models;
using NopStation.Plugin.Misc.AmazonPersonalize.Helpers;

namespace NopStation.Plugin.Misc.AmazonPersonalize.Services
{
    public class RecommenderService : IRecommenderService
    {
        #region Fields

        private readonly AmazonPersonalizeSettings _amazonPersonalizeSettings;
        private readonly ILogger _logger;
        private readonly IWorkContext _workContext;

        #endregion Fields

        #region Ctor

        public RecommenderService(AmazonPersonalizeSettings amazonPersonalizeSettings,
            ILogger logger,
            IWorkContext workContext)
        {
            _amazonPersonalizeSettings = amazonPersonalizeSettings;
            _logger = logger;
            _workContext = workContext;
        }

        #endregion Ctor

        #region Utilities

        private AmazonPersonalizeClient GetAmazonPersonalizeClient()
        {
            return new AmazonPersonalizeClient(_amazonPersonalizeSettings.AccessKey, _amazonPersonalizeSettings.SecretKey, AwsRegionHelper.GetRegionEndPoint(AwsRegionHelper.GetAwsRegion(_amazonPersonalizeSettings.AwsRegionId)));
        }

        #endregion Utilities

        #region Methods

        public virtual async Task<IList<RecommenderSummary>> GetRecommendersAsync(RecommenderSearchModel recommenderSearchModel)
        {
            var recommenders = new List<RecommenderSummary>();
            try
            {
                var listRecommenderRequest = new ListRecommendersRequest();
                listRecommenderRequest.DatasetGroupArn = _amazonPersonalizeSettings.DataSetGroupArn;

                var recommendersResponse = await GetAmazonPersonalizeClient().ListRecommendersAsync(listRecommenderRequest);
                recommenders = recommendersResponse.Recommenders;
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync(ex.Message, ex, await _workContext.GetCurrentCustomerAsync());
            }
            return recommenders;
        }

        #endregion Methods
    }
}