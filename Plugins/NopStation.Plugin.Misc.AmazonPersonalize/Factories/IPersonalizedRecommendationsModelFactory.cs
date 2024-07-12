using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using NopStation.Plugin.Misc.AmazonPersonalize.Domains;
using NopStation.Plugin.Misc.AmazonPersonalize.Models;

namespace NopStation.Plugin.Misc.AmazonPersonalize.Factories
{
    public interface IPersonalizedRecommendationsModelFactory
    {
        Task<RecommendedForYouModel> PrepareRecommendedForYouModelAsync(string[] itemIds);
        Task<RecommendationListModel> PrepareRecommendationListModelAsync(IList<Recommendation> recommenders, int productId = 0);
        Task<RecommendationModel> PrepareRecommendationModelAsync(Recommendation recommender, Customer customer, AdditionalData additionalData);
    }
}
