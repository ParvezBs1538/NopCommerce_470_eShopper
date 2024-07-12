using System.Collections.Generic;
using System.Threading.Tasks;
using NopStation.Plugin.Misc.AmazonPersonalize.Domains;

namespace NopStation.Plugin.Misc.AmazonPersonalize.Services
{
    public interface IRecommendationService
    {
        Task<IList<Recommendation>> GetAllRecommendationAsync(List<int> widgetZoneIds, bool? active);
        Task<Recommendation> GetRecommendationByIdAsync(int recommendationId);
    }
}