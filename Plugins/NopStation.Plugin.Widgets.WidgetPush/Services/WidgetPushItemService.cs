using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Data;
using Nop.Services.Stores;
using NopStation.Plugin.Widgets.WidgetPush.Domains;
using NopStation.Plugin.Widgets.WidgetPush.Services.Cache;

namespace NopStation.Plugin.Widgets.WidgetPush.Services
{
    public class WidgetPushItemService : IWidgetPushItemService
    {
        #region Fields

        private readonly IRepository<WidgetPushItem> _widgetPushItemRepository;
        private readonly IStaticCacheManager _cacheManager;
        private readonly IStoreContext _storeContext;
        private readonly IStoreMappingService _storeMappingService;

        #endregion

        #region Ctor

        public WidgetPushItemService(IRepository<WidgetPushItem> widgetPushItemRepository,
            IStaticCacheManager cacheManager,
            IStoreContext storeContext,
            IStoreMappingService storeMappingService)
        {
            _widgetPushItemRepository = widgetPushItemRepository;
            _cacheManager = cacheManager;
            _storeContext = storeContext;
            _storeMappingService = storeMappingService;
        }

        #endregion

        #region Methods

        public async Task DeleteWidgetPushItemAsync(WidgetPushItem widgetPushItem)
        {
            await _widgetPushItemRepository.DeleteAsync(widgetPushItem);
            await _cacheManager.RemoveByPrefixAsync(ModelCacheDefaults.WidgetPushZonesPrefixCacheKey);
        }

        public async Task InsertWidgetPushItemAsync(WidgetPushItem widgetPushItem)
        {
            await _widgetPushItemRepository.InsertAsync(widgetPushItem);
            await _cacheManager.RemoveByPrefixAsync(ModelCacheDefaults.WidgetPushZonesPrefixCacheKey);
        }

        public async Task UpdateWidgetPushItemAsync(WidgetPushItem widgetPushItem)
        {
            await _widgetPushItemRepository.UpdateAsync(widgetPushItem);
            await _cacheManager.RemoveByPrefixAsync(ModelCacheDefaults.WidgetPushZonesPrefixCacheKey);
        }

        public async Task<WidgetPushItem> GetWidgetPushItemByIdAsync(int widgetPushItemId)
        {
            if (widgetPushItemId == 0)
                return null;

            return await _widgetPushItemRepository.GetByIdAsync(widgetPushItemId, cache => default);
        }

        public async Task<IPagedList<WidgetPushItem>> GetAllWidgetPushItemsAsync(string widgetZone = "", bool? active = null,
            DateTime? startDate = null, DateTime? endDate = null, int storeId = 0, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _widgetPushItemRepository.Table;

            if (!string.IsNullOrWhiteSpace(widgetZone))
                query = query.Where(x => x.WidgetZone == widgetZone);

            if (active.HasValue)
                query = query.Where(x => x.Active == active.Value);

            if (startDate.HasValue)
                query = query.Where(x => !x.DisplayStartDateUtc.HasValue || x.DisplayStartDateUtc.Value >= startDate);

            if (endDate.HasValue)
                query = query.Where(x => !x.DisplayEndDateUtc.HasValue || x.DisplayEndDateUtc.Value <= endDate);

            //apply store mapping constraints
            query = await _storeMappingService.ApplyStoreMapping(query, storeId);

            query = query.OrderBy(x => x.DisplayOrder);

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        public async Task<IList<string>> GetAllWidgetZonesAsync()
        {
            var key = string.Format(ModelCacheDefaults.WidgetPushZonesModelKey, (await _storeContext.GetCurrentStoreAsync()).Id);
            var cacheKey = new CacheKey(key, ModelCacheDefaults.WidgetPushZonesPrefixCacheKey);
            var cachedZones = await _cacheManager.GetAsync(cacheKey, () =>
            {
                var zones = _widgetPushItemRepository.Table.Select(x => x.WidgetZone).Distinct().ToList();
                return zones;
            });

            return cachedZones;
        }

        #endregion
    }
}
