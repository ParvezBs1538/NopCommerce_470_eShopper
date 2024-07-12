using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using NopStation.Plugin.Misc.AlgoliaSearch.Domains;

namespace NopStation.Plugin.Misc.AlgoliaSearch.Services
{
    public class AlgoliaUpdatableItemService : IAlgoliaUpdatableItemService
    {
        #region Fields

        private readonly IRepository<AlgoliaUpdatableItem> _algoliaUpdatableItemRepository;
        private readonly IRepository<ProductCategory> _productCategoryRepository;
        private readonly IRepository<ProductManufacturer> _productManufacturerRepository;

        #endregion

        #region Ctor

        public AlgoliaUpdatableItemService(IRepository<AlgoliaUpdatableItem> algoliaUpdatableItemRepository,
            IRepository<ProductCategory> productCategoryRepository,
            IRepository<ProductManufacturer> productManufacturerRepository)
        {
            _algoliaUpdatableItemRepository = algoliaUpdatableItemRepository;
            _productCategoryRepository = productCategoryRepository;
            _productManufacturerRepository = productManufacturerRepository;
        }

        #endregion

        #region Methods

        public async Task DeleteAlgoliaUpdatableItemsByProductsAsync(IPagedList<Product> products = null)
        {
            if (products == null)
            {
                return;
            }
            var productIds = products.Select(x => x.Id).ToList();
            var categoryIds = from pcm in _productCategoryRepository.Table
                              where productIds.Contains(pcm.ProductId)
                              select pcm.CategoryId;
            var manufacturerIds = from pmm in _productManufacturerRepository.Table
                                  where productIds.Contains(pmm.ProductId)
                                  select pmm.ManufacturerId;
            var vendorIds = products.Select(x => x.VendorId).ToList();

            if (vendorIds != null && vendorIds.Contains(0))
                vendorIds.Remove(0);

            await _algoliaUpdatableItemRepository.DeleteAsync(record => (productIds.Contains(record.EntityId) && record.EntityName == "Product"));
            await _algoliaUpdatableItemRepository.DeleteAsync(record => (categoryIds.Contains(record.EntityId) && record.EntityName == "Category"));
            await _algoliaUpdatableItemRepository.DeleteAsync(record => (manufacturerIds.Contains(record.EntityId) && record.EntityName == "Manufacturer"));
            await _algoliaUpdatableItemRepository.DeleteAsync(record => (vendorIds.Contains(record.EntityId) && record.EntityName == "Vendor"));
        }
        public async Task DeleteAlgoliaUpdatableItemAsync(AlgoliaUpdatableItem algoliaUpdatableItem)
        {
            await _algoliaUpdatableItemRepository.DeleteAsync(algoliaUpdatableItem);
        }

        public async Task InsertAlgoliaUpdatableItemAsync(AlgoliaUpdatableItem algoliaUpdatableItem)
        {
            await _algoliaUpdatableItemRepository.InsertAsync(algoliaUpdatableItem);
        }

        public async Task UpdateAlgoliaUpdatableItemAsync(AlgoliaUpdatableItem algoliaUpdatableItem)
        {
            await _algoliaUpdatableItemRepository.UpdateAsync(algoliaUpdatableItem);
        }

        public async Task<AlgoliaUpdatableItem> GetAlgoliaUpdatableItemByIdAsync(int algoliaUpdatableItemId)
        {
            if (algoliaUpdatableItemId == 0)
                return null;

            return await _algoliaUpdatableItemRepository.GetByIdAsync(algoliaUpdatableItemId);
        }

        public async Task<IPagedList<AlgoliaUpdatableItem>> SearchAlgoliaUpdatableItemsAsync(string entityName = "", int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var algoliaUpdatableItems = _algoliaUpdatableItemRepository.Table;

            if (!string.IsNullOrWhiteSpace(entityName))
                algoliaUpdatableItems = algoliaUpdatableItems.Where(e => e.EntityName == entityName);

            algoliaUpdatableItems = algoliaUpdatableItems.OrderByDescending(e => e.Id);

            return await algoliaUpdatableItems.ToPagedListAsync(pageIndex, pageSize);
        }

        #endregion
    }
}
