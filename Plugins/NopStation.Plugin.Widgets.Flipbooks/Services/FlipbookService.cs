using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using NopStation.Plugin.Misc.Core.Caching;
using NopStation.Plugin.Widgets.Flipbooks.Domains;
using NopStation.Plugin.Widgets.Flipbooks.Services.Cache;
using Nop.Services.Security;
using Nop.Services.Stores;

namespace NopStation.Plugin.Widgets.Flipbooks.Services
{
    public class FlipbookService : IFlipbookService
    {
        #region Fields

        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<Flipbook> _flipbookRepository;
        private readonly IRepository<FlipbookContent> _flipbookContentRepository;
        private readonly IRepository<FlipbookContentProduct> _flipbookContentProductRepository;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IAclService _aclService;
        private readonly IWorkContext _workContext;
        private readonly IStaticCacheManager _staticCacheManager;

        #endregion

        #region Ctor

        public FlipbookService(IRepository<Product> productRepository,
            IRepository<Flipbook> flipbookRepository,
            IRepository<FlipbookContent> flipbookContentRepository,
            IRepository<FlipbookContentProduct> flipbookContentProductRepository,
            IStoreMappingService storeMappingService,
            IAclService aclService,
            IWorkContext workContext,
            IStaticCacheManager staticCacheManager)
        {
            _productRepository = productRepository;
            _flipbookRepository = flipbookRepository;
            _flipbookContentRepository = flipbookContentRepository;
            _flipbookContentProductRepository = flipbookContentProductRepository;
            _storeMappingService = storeMappingService;
            _aclService = aclService;
            _workContext = workContext;
            _staticCacheManager = staticCacheManager;
        }

        #endregion
        
        #region Methods

        #region Flipbooks

        public async Task DeleteFlipbookAsync(Flipbook flipbook)
        {
            await _flipbookRepository.DeleteAsync(flipbook);
        }

        public async Task<Flipbook> GetFlipbookByIdAsync(int flipbookId)
        {
            if (flipbookId == 0)
                return null;

            return await _flipbookRepository.GetByIdAsync(flipbookId, cache =>
                _staticCacheManager.PrepareKeyForDefaultCache(NopStationEntityCacheDefaults<Flipbook>.ByIdCacheKey, flipbookId));
        }

        public async Task InsertFlipbookAsync(Flipbook flipbook)
        {
            await _flipbookRepository.InsertAsync(flipbook);
        }

        public async Task UpdateFlipbookAsync(Flipbook flipbook)
        {
            await _flipbookRepository.UpdateAsync(flipbook);
        }

        public async Task<IPagedList<Flipbook>> SearchFlipbooksAsync(string name = null, bool? includeInTopMenu = null, 
            bool? active = null, int storeId = 0, bool showHidden = false, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(FlipbooksCacheDefaults.FlipbooksAllKey,
                name, includeInTopMenu, storeId, active, showHidden, pageIndex, pageSize);

            return await _staticCacheManager.GetAsync(cacheKey, async () =>
            {
                var query = from f in _flipbookRepository.Table
                            where !f.Deleted &&
                            (!includeInTopMenu.HasValue || f.IncludeInTopMenu == includeInTopMenu.Value) &&
                            (!active.HasValue || f.Active == active.Value)
                            select f;

                if (!string.IsNullOrWhiteSpace(name))
                    query = query.Where(f => f.Name.Contains(name));

                if (active.HasValue)
                {
                    query = from f in query
                            where f.Active &&
                            LinqToDB.Sql.Between(DateTime.UtcNow, f.AvailableStartDateTimeUtc ?? DateTime.MinValue, f.AvailableEndDateTimeUtc ?? DateTime.MaxValue)
                            select f;
                }

                //apply ACL constraints
                if (!showHidden)
                {
                    var customer = await _workContext.GetCurrentCustomerAsync();
                    query = await _aclService.ApplyAcl(query, customer);
                }

                //apply store mapping constraints
                query = await _storeMappingService.ApplyStoreMapping(query, storeId);

                query = query.OrderBy(f => f.DisplayOrder);

                return await query.ToPagedListAsync(pageIndex, pageSize);
            });
        }

        #endregion

        #region Contents

        public async Task DeleteFlipbookContentAsync(FlipbookContent flipbookContent)
        {
            await _flipbookContentRepository.DeleteAsync(flipbookContent);
        }

        public async Task<FlipbookContent> GetFlipbookContentByIdAsync(int flipbookContentId)
        {
            if (flipbookContentId == 0)
                return null;

            return await _flipbookContentRepository.GetByIdAsync(flipbookContentId, cache =>
                _staticCacheManager.PrepareKeyForDefaultCache(NopStationEntityCacheDefaults<FlipbookContent>.ByIdCacheKey, flipbookContentId));
        }

        public async Task InsertFlipbookContentAsync(FlipbookContent flipbookContent)
        {
            await _flipbookContentRepository.InsertAsync(flipbookContent);
        }

        public async Task UpdateFlipbookContentAsync(FlipbookContent flipbookContent)
        {
            await _flipbookContentRepository.UpdateAsync(flipbookContent);
        }

        public async Task<IList<FlipbookContent>> GetFlipbookContentsByFlipbookIdAsync(int flipbookId)
        {
            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(FlipbooksCacheDefaults.FlipbookContentsByFlipbookIdKey, flipbookId);
            
            return await _staticCacheManager.GetAsync(cacheKey, async () =>
            {
                var query = _flipbookContentRepository.Table
                    .Where(x => x.FlipbookId == flipbookId)
                    .OrderBy(point => point.DisplayOrder);

                return await query.ToListAsync();
            });
        }

        #endregion

        #region Content products

        public async Task<IList<FlipbookContentProduct>> GetFlipbookContentProductsByContentIdAsync(int flipbookContentId)
        {
            if (flipbookContentId == 0)
                return new List<FlipbookContentProduct>();

            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(FlipbooksCacheDefaults.FlipbookContentProductsByFlipbookContentIdKey, flipbookContentId);

            return await _staticCacheManager.GetAsync(cacheKey, async () =>
            {
                var query = from p in _productRepository.Table
                            join fc in _flipbookContentProductRepository.Table on p.Id equals fc.ProductId
                            where fc.FlipbookContentId == flipbookContentId && !p.Deleted
                            select fc;

                query = query.OrderBy(point => point.DisplayOrder);

                return await query.ToListAsync();
            });
        }

        public async Task<IPagedList<Product>> GetProductsByFlipbookContentIdAsync(int flipbookContentId, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(FlipbooksCacheDefaults.ProductsByFlipbookContentIdKey, 
                flipbookContentId, pageIndex, pageSize);

            return await _staticCacheManager.GetAsync(cacheKey, async () =>
            {
                var query = from cp in _flipbookContentProductRepository.Table
                            join p in _productRepository.Table
                            on cp.ProductId equals p.Id
                            where !p.Deleted && p.Published && cp.FlipbookContentId == flipbookContentId
                            orderby cp.DisplayOrder
                            select p;

                return await query.ToPagedListAsync(pageIndex, pageSize);
            });
        }

        public async Task<FlipbookContentProduct> GetFlipbookContentProductAsync(int flipbookContentId, int productId)
        {
            if (flipbookContentId == 0 || productId == 0)
                return null;

            var query = _flipbookContentProductRepository.Table
                .Where(x => x.FlipbookContentId == flipbookContentId && x.ProductId == productId);

            return await query.FirstOrDefaultAsync();
        }

        public async Task DeleteFlipbookContentProductAsync(FlipbookContentProduct flipbookContentProduct)
        {
            await _flipbookContentProductRepository.DeleteAsync(flipbookContentProduct);
        }

        public async Task<FlipbookContentProduct> GetFlipbookContentProductByIdAsync(int flipbookContentProductId)
        {
            if (flipbookContentProductId == 0)
                return null;

            return await _flipbookContentProductRepository.GetByIdAsync(flipbookContentProductId, cache =>
                _staticCacheManager.PrepareKeyForDefaultCache(NopStationEntityCacheDefaults<FlipbookContentProduct>.ByIdCacheKey, flipbookContentProductId));
        }

        public async Task InsertFlipbookContentProductAsync(FlipbookContentProduct flipbookContentProduct)
        {
            await _flipbookContentProductRepository.InsertAsync(flipbookContentProduct);
        }

        public async Task UpdateFlipbookContentProductAsync(FlipbookContentProduct flipbookContentProduct)
        {
            await _flipbookContentProductRepository.UpdateAsync(flipbookContentProduct);
        }

        #endregion

        #endregion
    }
}
