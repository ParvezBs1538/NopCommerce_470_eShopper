using System.Threading.Tasks;
using NopStation.Plugin.Misc.AlgoliaSearch.Models;

namespace NopStation.Plugin.Misc.AlgoliaSearch.Factories
{
    public interface IAlgoliaCatalogModelFactory
    {
        Task<SearchModel> PrepareSearchModelAsync(SearchModel model, AlgoliaPagingFilteringModel command);
    }
}
