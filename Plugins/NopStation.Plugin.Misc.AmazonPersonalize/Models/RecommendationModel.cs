using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Catalog;

namespace NopStation.Plugin.Misc.AmazonPersonalize.Models
{
    public record RecommendationModel : BaseNopEntityModel
    {
        public RecommendationModel()
        {
            Products = new List<ProductOverviewModel>();
        }
        public string Title { get; set; }
        public IList<ProductOverviewModel> Products { get; set; }
    }
}
