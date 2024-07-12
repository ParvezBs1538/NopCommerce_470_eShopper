using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Catalog;

namespace NopStation.Plugin.Misc.AmazonPersonalize.Models
{
    public record RecommendedForYouModel : BaseNopModel
    {
        public RecommendedForYouModel()
        {
            RecommendedProducts = new List<ProductOverviewModel>();
        }
        public IList<ProductOverviewModel> RecommendedProducts { get; set; }
    }
}