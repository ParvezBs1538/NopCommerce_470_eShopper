using System.Threading.Tasks;
using NopStation.Plugin.Misc.AlgoliaSearch.Areas.Admin.Models;

namespace NopStation.Plugin.Misc.AlgoliaSearch.Areas.Admin.Factories
{
    public interface IAlgoliaSearchModelFactory
    {
        Task<ConfigurationModel> PrepareConfigurationModelAsync();

        Task<UpdatableItemListModel> PrepareUpdatableItemListModelAsync(UpdatableItemSearchModel searchModel);
    }
}