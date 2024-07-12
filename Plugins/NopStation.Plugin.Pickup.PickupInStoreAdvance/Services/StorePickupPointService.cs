using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Data;
using NopStation.Plugin.Pickup.PickupInStoreAdvance.Domain;

namespace NopStation.Plugin.Pickup.PickupInStoreAdvance.Services
{
    public partial class StorePickupPointService : IStorePickupPointService
    {
        #region Constants

        private readonly CacheKey _pickupPointAllKey = new CacheKey("Nop.pickuppointadvance.all-{0}");
        private const string PICKUP_POINT_PATTERN_KEY = "Nop.pickuppointadvance.";

        #endregion

        #region Fields

        private readonly IRepository<StorePickupPoint> _storePickupPointRepository;
        private readonly IStaticCacheManager _staticCacheManager;

        #endregion

        #region Ctor

        public StorePickupPointService(IRepository<StorePickupPoint> storePickupPointRepository,
            IStaticCacheManager staticCacheManager)
        {
            _storePickupPointRepository = storePickupPointRepository;
            _staticCacheManager = staticCacheManager;
        }

        #endregion

        #region Methods

        public virtual async Task<IPagedList<StorePickupPoint>> GetAllStorePickupPointsAsync(int storeId = 0, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var rez = await _storePickupPointRepository.GetAllAsync(query =>
            {
                if (storeId > 0)
                    query = query.Where(point => point.StoreId == storeId || point.StoreId == 0);
                query = query.OrderBy(point => point.DisplayOrder).ThenBy(point => point.Name);

                return query;
            }, cache => cache.PrepareKey(_pickupPointAllKey, storeId));

            return new PagedList<StorePickupPoint>(rez, pageIndex, pageSize);
        }

        public virtual async Task<StorePickupPoint> GetStorePickupPointByIdAsync(int pickupPointId)
        {
            if (pickupPointId == 0)
                return null;

            return await _storePickupPointRepository.GetByIdAsync(pickupPointId);
        }

        public virtual async Task InsertStorePickupPointAsync(StorePickupPoint pickupPoint)
        {
            if (pickupPoint == null)
                throw new ArgumentNullException(nameof(pickupPoint));

            await _storePickupPointRepository.InsertAsync(pickupPoint);
            await _staticCacheManager.RemoveByPrefixAsync(PICKUP_POINT_PATTERN_KEY);
        }

        public virtual async Task UpdateStorePickupPointAsync(StorePickupPoint pickupPoint)
        {
            await _storePickupPointRepository.UpdateAsync(pickupPoint, false);
            await _staticCacheManager.RemoveByPrefixAsync(PICKUP_POINT_PATTERN_KEY);
        }

        public virtual async Task DeleteStorePickupPointAsync(StorePickupPoint pickupPoint)
        {
            await _storePickupPointRepository.DeleteAsync(pickupPoint, false);
            await _staticCacheManager.RemoveByPrefixAsync(PICKUP_POINT_PATTERN_KEY);
        }

        #endregion
    }
}