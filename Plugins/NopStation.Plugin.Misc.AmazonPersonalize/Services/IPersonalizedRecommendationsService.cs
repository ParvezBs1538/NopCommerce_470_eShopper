using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.PersonalizeRuntime.Model;
using NopStation.Plugin.Misc.AmazonPersonalize.Domains;

namespace NopStation.Plugin.Misc.AmazonPersonalize.Services
{
    public interface IPersonalizedRecommendationsService
    {
        Task<List<PredictedItem>> GetRecommendationResults(string userId, Recommendation recommendation, string itemId = null);
    }
}