using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Stores;
using Nop.Core.Events;
using Nop.Data;
using NopStation.Plugin.Widgets.ProductTabs.Domains;

namespace NopStation.Plugin.Widgets.ProductTabs.Services
{
    public class ProductTabService : IProductTabService
    {
        #region Fields

        private readonly IStaticCacheManager _cacheManager;
        private readonly IRepository<ProductTab> _productTabRepository;
        private readonly IRepository<ProductTabItem> _productTabItemRepository;
        private readonly IRepository<ProductTabItemProduct> _productTabItemProductRepository;
        private readonly IRepository<StoreMapping> _storeMappingRepository;
        private readonly CatalogSettings _catalogSettings;
        private readonly IEventPublisher _eventPublisher;

        #endregion

        #region Ctor

        public ProductTabService(
            IStaticCacheManager cacheManager,
            IRepository<ProductTab> productTabRepository,
            IRepository<ProductTabItem> productTabItemRepository,
            IRepository<ProductTabItemProduct> productTabItemProductRepository,
            IRepository<StoreMapping> storeMappingRepository,
            CatalogSettings catalogSettings,
            IEventPublisher eventPublisher)
        {
            _cacheManager = cacheManager;
            _productTabRepository = productTabRepository;
            _productTabItemRepository = productTabItemRepository;
            _productTabItemProductRepository = productTabItemProductRepository;
            _storeMappingRepository = storeMappingRepository;
            _catalogSettings = catalogSettings;
            _eventPublisher = eventPublisher;
        }

        #endregion

        #region Methods

        #region Product tabs

        public async Task DeleteProductTabAsync(ProductTab productTab)
        {
            await _productTabRepository.DeleteAsync(productTab);
        }

        public async Task InsertProductTabAsync(ProductTab productTab)
        {
            await _productTabRepository.InsertAsync(productTab);
        }

        public async Task UpdateProductTabAsync(ProductTab productTab)
        {
            await _productTabRepository.UpdateAsync(productTab);
        }

        public async Task<ProductTab> GetProductTabByIdAsync(int productTabId)
        {
            if (productTabId == 0)
                return null;

            return await _productTabRepository.GetByIdAsync(productTabId, cache => default);
        }

        public async Task<IPagedList<ProductTab>> GetAllProductTabsAsync(List<int> widgetZoneIds = null, bool hasItemsOnly = false,
            int storeId = 0, bool? active = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _productTabRepository.Table;

            if (widgetZoneIds != null && widgetZoneIds.Any())
                query = query.Where(productTab => widgetZoneIds.Contains(productTab.WidgetZoneId));

            if (active.HasValue)
                query = query.Where(productTab => productTab.Active == active.Value);

            if (storeId > 0 && !_catalogSettings.IgnoreStoreLimitations)
            {
                var sm = _storeMappingRepository.Table
                    .Where(x => x.EntityName == nameof(ProductTab) && x.StoreId == storeId)
                    .ToList();

                query = query.Where(x => !x.LimitedToStores || sm.Any(y => y.EntityId == x.Id));
            }

            query = query.OrderBy(e => e.DisplayOrder);

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        #endregion

        #region Product tab items

        public async Task DeleteProductTabItemAsync(ProductTabItem productTabItem)
        {
            await _productTabItemRepository.DeleteAsync(productTabItem);
        }

        public async Task UpdateProductTabItemAsync(ProductTabItem productTabItem)
        {
            await _productTabItemRepository.UpdateAsync(productTabItem);
        }

        #endregion

        #region Product tab item products

        public async Task<ProductTabItemProduct> GetProductTabItemProductByIdAsync(int productTabItemProductId)
        {
            if (productTabItemProductId == 0)
                return null;

            return await _productTabItemProductRepository.GetByIdAsync(productTabItemProductId, cache => default);
        }

        public async Task DeleteProductTabItemProductAsync(ProductTabItemProduct productTabItemProduct)
        {
            await _productTabItemProductRepository.DeleteAsync(productTabItemProduct);
        }

        public async Task UpdateProductTabItemProductAsync(ProductTabItemProduct productTabItemProduct)
        {
            await _productTabItemProductRepository.UpdateAsync(productTabItemProduct);
        }

        public async Task<ProductTabItem> GetProductTabItemByIdAsync(int productTabItemId)
        {
            if (productTabItemId == 0)
                return null;

            return await _productTabItemRepository.GetByIdAsync(productTabItemId, cache => default);
        }

        public List<ProductTabItem> GetProductTabItemsByProductTabId(int productTabId)
        {
            if (productTabId == 0)
                return null;

            var query = _productTabItemRepository.Table.Where(x => x.ProductTabId == productTabId);
            return query.ToList();
        }

        public List<ProductTabItemProduct> GetProductTabItemProductsByProductTabItemId(int productTabItemId)
        {
            if (productTabItemId == 0)
                return null;

            var query = _productTabItemProductRepository.Table.Where(x => x.ProductTabItemId == productTabItemId);
            return query.ToList();
        }

        public async Task InsertProductTabItemAsync(ProductTabItem productTabItem)
        {
            await _productTabItemRepository.InsertAsync(productTabItem);
        }

        public async Task InsertProductTabItemProductAsync(ProductTabItemProduct productTabItemProduct)
        {
            await _productTabItemProductRepository.InsertAsync(productTabItemProduct);
        }

        #endregion

        #endregion
    }
}
