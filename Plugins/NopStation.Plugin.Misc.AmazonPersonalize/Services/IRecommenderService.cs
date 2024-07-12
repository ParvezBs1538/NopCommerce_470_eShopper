using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.Personalize.Model;
using NopStation.Plugin.Misc.AmazonPersonalize.Areas.Admin.Models;

namespace NopStation.Plugin.Misc.AmazonPersonalize.Services
{
    public interface IRecommenderService
    {
        Task<IList<RecommenderSummary>> GetRecommendersAsync(RecommenderSearchModel recommenderSearchModel);
    }
}