using System.Collections.Generic;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Models.Catalog;

namespace NopStation.Plugin.Misc.AlgoliaSearch.Models
{
    public class SearchModel
    {
        public SearchModel()
        {
            PagingFilteringContext = new AlgoliaPagingFilteringModel();
            Products = new List<ProductOverviewModel>();
        }

        [NopResourceDisplayName("NopStation.AlgoliaSearch.Search.SearchTerm")]
        public string q { get; set; }

        public AlgoliaPagingFilteringModel PagingFilteringContext { get; set; }
        public IList<ProductOverviewModel> Products { get; set; }
    }
}
