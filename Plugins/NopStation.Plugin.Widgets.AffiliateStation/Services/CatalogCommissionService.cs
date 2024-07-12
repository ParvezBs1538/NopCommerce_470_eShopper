using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Data;
using NopStation.Plugin.Widgets.AffiliateStation.Domains;
using NopStation.Plugin.Widgets.AffiliateStation.Services.Cache;
using NopStation.Plugin.Misc.Core.Caching;

namespace NopStation.Plugin.Widgets.AffiliateStation.Services
{
    public class CatalogCommissionService : ICatalogCommissionService
    {
        #region Fields

        private readonly IRepository<CatalogCommission> _catalogCommissionRepository;
        private readonly IStaticCacheManager _staticCacheManager;

        #endregion

        #region Ctor

        public CatalogCommissionService(IRepository<CatalogCommission> catalogCommissionRepository,
            IStaticCacheManager staticCacheManager)
        {
            _catalogCommissionRepository = catalogCommissionRepository;
            _staticCacheManager = staticCacheManager;
        }

        #endregion

        #region Methods

        public async Task DeleteCatalogCommissionAsync(CatalogCommission catalogCommission)
        {
            await _catalogCommissionRepository.DeleteAsync(catalogCommission);
        }

        public async Task InsertCatalogCommissionAsync(CatalogCommission catalogCommission)
        {
            await _catalogCommissionRepository.InsertAsync(catalogCommission);
        }

        public async Task<CatalogCommission> GetCatalogCommissionByIdAsync(int catalogCommissionId)
        {
            if (catalogCommissionId == 0)
                return null;

            return await _catalogCommissionRepository.GetByIdAsync(catalogCommissionId, cache => 
                _staticCacheManager.PrepareKeyForDefaultCache(NopStationEntityCacheDefaults<CatalogCommission>.ByIdCacheKey, catalogCommissionId));
        }

        public async Task<CatalogCommission> GetCatalogCommissionByEntityAsync(BaseEntity baseEntity)
        {
            if (baseEntity == null)
                return null;

            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(AffiliateStationCacheDefaults.CatalogCommissionByBaseEntityKey, baseEntity.Id, baseEntity.GetType().Name);
            
            return await _staticCacheManager.GetAsync(cacheKey, async () =>
            {
                var entityName = baseEntity.GetType().Name;
                var query = _catalogCommissionRepository.Table;

                return await query.FirstOrDefaultAsync(cc => cc.EntityId == baseEntity.Id && cc.EntityName == entityName);
            });
        }

        public async Task<IPagedList<CatalogCommission>> GetAllCatalogCommissionsAsync(string entityName = null,
            int entityId = 0, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(AffiliateStationCacheDefaults.CatalogCommissionAllKey, 
                entityName, entityId, pageIndex, pageSize);

            return await _staticCacheManager.GetAsync(cacheKey, async () =>
            {
                var query = from q in _catalogCommissionRepository.Table
                            where !string.IsNullOrWhiteSpace(entityName) || (q.EntityName == entityName && q.EntityId == entityId)
                            orderby q.Id descending
                            select q;

                return await query.ToPagedListAsync(pageIndex, pageSize);
            });
        }

        public async Task UpdateCatalogCommissionAsync(CatalogCommission catalogCommission)
        {
            await _catalogCommissionRepository.UpdateAsync(catalogCommission);
        }

        #endregion
    }
}
