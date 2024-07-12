using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Data;
using Nop.Services.Stores;
using NopStation.Plugin.Misc.Core.Caching;
using NopStation.Plugin.Widgets.VendorShop.Domains.SliderVendorShop;
using NopStation.Plugin.Widgets.VendorShop.Services.Cache;

namespace NopStation.Plugin.Widgets.VendorShop.Services
{
    public class SliderService : ISliderService
    {
        #region Fields

        private readonly IStaticCacheManager _cacheManager;
        private readonly IRepository<SliderItem> _sliderItemRepository;
        private readonly IRepository<Slider> _sliderRepository;
        private readonly IStoreMappingService _storeMappingService;

        #endregion

        #region Ctor

        public SliderService(IRepository<Slider> sliderRepository,
            IRepository<SliderItem> sliderItemRepository,
            IStaticCacheManager cacheManager,
            IStoreMappingService storeMappingService)
        {
            _sliderRepository = sliderRepository;
            _sliderItemRepository = sliderItemRepository;
            _storeMappingService = storeMappingService;
            _cacheManager = cacheManager;
        }

        #endregion

        #region Slider

        public virtual async Task<IPagedList<Slider>> GetAllSlidersAsync(List<int> widgetZoneIds = null, int storeId = 0, int vendorId = 0,
            bool? active = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var cacheKey = _cacheManager.PrepareKeyForDefaultCache(AnywhereSliderCacheDefaults.GetSlidersAllKey(vendorId),
                vendorId, widgetZoneIds, storeId, active, pageIndex, pageSize);

            return await _cacheManager.GetAsync(cacheKey, async () =>
            {
                var query = from s in _sliderRepository.Table
                            where !s.Deleted &&
                            (!active.HasValue || s.Active == active.Value)
                            select s;

                if (widgetZoneIds != null && widgetZoneIds.Any())
                    query = from s in query
                            where widgetZoneIds.Contains(s.WidgetZoneId)
                            select s;

                query = await _storeMappingService.ApplyStoreMapping(query, storeId);
                if (vendorId > 0)
                {
                    query = query.Where(x => x.VendorId == vendorId);
                }
                query = query.OrderBy(carousel => carousel.DisplayOrder);

                return await query.ToPagedListAsync(pageIndex, pageSize);
            });
        }

        public virtual async Task<Slider> GetSliderByIdAsync(int sliderId)
        {
            if (sliderId == 0)
                return null;

            return await _sliderRepository.GetByIdAsync(sliderId, cache =>
                _cacheManager.PrepareKeyForDefaultCache(NopStationEntityCacheDefaults<Slider>.ByIdCacheKey, sliderId));
        }

        public virtual async Task InsertSliderAsync(Slider slider)
        {
            await _sliderRepository.InsertAsync(slider);
        }

        public virtual async Task UpdateSliderAsync(Slider slider)
        {
            await _sliderRepository.UpdateAsync(slider);
        }

        public virtual async Task DeleteSliderAsync(Slider slider)
        {
            await _sliderRepository.DeleteAsync(slider);
        }

        #endregion

        #region Slider items

        public virtual async Task<IPagedList<SliderItem>> GetSliderItemsBySliderIdAsync(int sliderId, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var slider = await GetSliderByIdAsync(sliderId);

            var cacheKey = _cacheManager.PrepareKeyForDefaultCache(AnywhereSliderCacheDefaults.GetSliderItemsBySliderIdKey(slider.VendorId), slider.VendorId, sliderId, pageIndex, pageSize);

            return await _cacheManager.GetAsync(cacheKey, async () =>
            {
                var query = from si in _sliderItemRepository.Table
                            where si.SliderId == sliderId
                            orderby si.DisplayOrder
                            select si;

                return await query.ToPagedListAsync(pageIndex, pageSize);
            });
        }

        public virtual async Task<SliderItem> GetSliderItemByIdAsync(int sliderItemId)
        {
            if (sliderItemId == 0)
                return null;

            return await _sliderItemRepository.GetByIdAsync(sliderItemId, cache =>
                _cacheManager.PrepareKeyForDefaultCache(NopStationEntityCacheDefaults<SliderItem>.ByIdCacheKey, sliderItemId));
        }

        public async Task InsertSliderItemAsync(SliderItem sliderItem)
        {
            await _sliderItemRepository.InsertAsync(sliderItem);
        }

        public virtual async Task UpdateSliderItemAsync(SliderItem sliderItem)
        {
            await _sliderItemRepository.UpdateAsync(sliderItem);
        }

        public virtual async Task DeleteSliderItemAsync(SliderItem sliderItem)
        {
            await _sliderItemRepository.DeleteAsync(sliderItem);
        }

        #endregion
    }
}
