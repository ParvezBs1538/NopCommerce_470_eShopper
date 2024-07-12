using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Web.Models.Catalog;
using NopStation.Plugin.Misc.AlgoliaSearch.Areas.Admin.Models;
using NopStation.Plugin.Misc.AlgoliaSearch.Models;

namespace NopStation.Plugin.Misc.AlgoliaSearch.Factories
{
    public interface IAlgoliaHelperFactory
    {
        Task UploadProductsAsync(UploadProductModel model);

        Task UpdateIndicesAsync(ConfigurationModel model);

        void ClearIndex();

        Task UpdateAlgoliaItemAsync();

        Task<IPagedList<ProductOverviewModel>> SearchProductsAsync(string searchTerms = "", IList<int> cids = null,
            IList<int> mids = null, IList<int> vids = null, IList<FilteredGroupModel> specids = null,
            IList<FilteredGroupModel> attrids = null, IList<int> ratings = null, decimal? minPrice = null,
            decimal? maxPrice = null, int? orderby = null, int pageIndex = 0, int pageSize = int.MaxValue);

        Task<AlgoliaFilters> GetAlgoliaFiltersAsync(string searchTerms);
    }
}