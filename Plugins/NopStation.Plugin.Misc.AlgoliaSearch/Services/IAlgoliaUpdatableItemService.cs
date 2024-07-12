using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using NopStation.Plugin.Misc.AlgoliaSearch.Domains;

namespace NopStation.Plugin.Misc.AlgoliaSearch.Services
{
    public interface IAlgoliaUpdatableItemService
    {
        Task DeleteAlgoliaUpdatableItemAsync(AlgoliaUpdatableItem algoliaUpdatableItem);

        Task DeleteAlgoliaUpdatableItemsByProductsAsync(IPagedList<Product> products = null);

        Task InsertAlgoliaUpdatableItemAsync(AlgoliaUpdatableItem algoliaUpdatableItem);

        Task UpdateAlgoliaUpdatableItemAsync(AlgoliaUpdatableItem algoliaUpdatableItem);

        Task<AlgoliaUpdatableItem> GetAlgoliaUpdatableItemByIdAsync(int algoliaUpdatableItemId);

        Task<IPagedList<AlgoliaUpdatableItem>> SearchAlgoliaUpdatableItemsAsync(string entityName = "",
            int pageIndex = 0, int pageSize = int.MaxValue);
    }
}