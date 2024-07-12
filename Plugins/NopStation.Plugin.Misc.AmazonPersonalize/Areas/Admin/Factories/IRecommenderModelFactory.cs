using System.Threading.Tasks;
using Amazon.Personalize.Model;
using NopStation.Plugin.Misc.AmazonPersonalize.Areas.Admin.Models;

namespace NopStation.Plugin.Misc.AmazonPersonalize.Areas.Admin.Factories
{
    public interface IRecommenderModelFactory
    {
        Task<RecommenderListModel> PrepareRecommenderListModelAsync(RecommenderSearchModel searchModel);

        Task<RecommenderModel> PrepareRecommenderModelAsync(RecommenderModel model, RecommenderSummary recommenderSummary);
    }
}