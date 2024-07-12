using System.Collections.Generic;
using Nop.Web.Framework.Models;
namespace NopStation.Plugin.Misc.AmazonPersonalize.Models
{
    public record RecommendationListModel : BaseNopModel
    {
        public RecommendationListModel()
        {
            Recommendations = new List<RecommenderOverviewModel>();
        }
        public List<RecommenderOverviewModel> Recommendations { get; set; }
        public record RecommenderOverviewModel : BaseNopEntityModel
        {
            public string Title { get; set; }
            public int ProductId { get; set; }
        }
    }
}
