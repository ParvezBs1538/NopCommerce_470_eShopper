﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Stores;
using Nop.Core.Events;
using Nop.Data;
using Nop.Services.Stores;
using NopStation.Plugin.Widgets.OCarousels.Domains;

namespace NopStation.Plugin.Widgets.OCarousels.Services
{
    public class OCarouselService : IOCarouselService
    {
        #region Props

        private const string OCAROUSEL_ALL_KEY = "NS.OCarouselList.all";
        private const string OCAROUSEL_REGION_KEY = "NS.OCarouselList.region.{0}-{1}-{2}-{3}-{4}-{5}";
        private const string OCAROUSEL_PATTERN_KEY = "NS.OCarouselList.";

        #endregion

        #region Fields

        private readonly IStaticCacheManager _cacheManager;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IRepository<OCarousel> _carouselRepository;
        private readonly IRepository<OCarouselItem> _carouselItemRepository;
        private readonly IRepository<StoreMapping> _storeMappingRepository;
        private readonly CatalogSettings _catalogSettings;
        private readonly IEventPublisher _eventPublisher;

        #endregion

        #region Ctor

        public OCarouselService(IStaticCacheManager cacheManager,
            IStoreMappingService storeMappingService,
            IRepository<OCarousel> carouselRepository,
            IRepository<OCarouselItem> carouselItemRepository,
            IRepository<StoreMapping> storeMappingRepository,
            IEventPublisher eventPublisher,
            CatalogSettings catalogSettings)
        {
            _cacheManager = cacheManager;
            _storeMappingService = storeMappingService;
            _carouselRepository = carouselRepository;
            _carouselItemRepository = carouselItemRepository;
            _storeMappingRepository = storeMappingRepository;
            _eventPublisher = eventPublisher;
            _catalogSettings = catalogSettings;
        }

        #endregion

        #region Methods

        #region OCarousel

        public virtual async Task<IPagedList<OCarousel>> GetAllCarouselsAsync(List<int> widgetZoneIds = null, List<int> dataSources = null,
            int storeId = 0, bool? active = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _carouselRepository.Table.Where(x => !x.Deleted);
            if (widgetZoneIds != null && widgetZoneIds.Any())
                query = query.Where(carousel => widgetZoneIds.Contains(carousel.WidgetZoneId));

            if (dataSources != null && dataSources.Any())
                query = query.Where(carousel => dataSources.Contains(carousel.DataSourceTypeId));

            if (active.HasValue)
                query = query.Where(carousel => carousel.Active == active.Value);

            if (storeId > 0 && !_catalogSettings.IgnoreStoreLimitations)
            {
                query = from o in query
                        join sm in _storeMappingRepository.Table
                          on new { c1 = o.Id, c2 = nameof(OCarousel) } equals new { c1 = sm.EntityId, c2 = sm.EntityName } into carousel_sm
                        from sm in carousel_sm.DefaultIfEmpty()
                        where !o.LimitedToStores || storeId == sm.StoreId
                        select o;
            }

            query = query.OrderBy(carousel => carousel.DisplayOrder);
            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        public virtual async Task<OCarousel> GetCarouselByIdAsync(int carouselId)
        {
            if (carouselId == 0)
                return null;

            return await _carouselRepository.GetByIdAsync(carouselId, cache => default);
        }

        public virtual async Task InsertCarouselAsync(OCarousel oCarousel)
        {
            await _carouselRepository.InsertAsync(oCarousel);

            await _cacheManager.RemoveByPrefixAsync(OCAROUSEL_PATTERN_KEY);
        }

        public async virtual Task UpdateCarouselAsync(OCarousel oCarousel)
        {
            await _carouselRepository.UpdateAsync(oCarousel);

            await _cacheManager.RemoveByPrefixAsync(OCAROUSEL_PATTERN_KEY);
        }

        public async virtual Task DeleteCarouselAsync(OCarousel oCarousel)
        {
            oCarousel.Deleted = true;
            await _carouselRepository.UpdateAsync(oCarousel);

            await _cacheManager.RemoveByPrefixAsync(OCAROUSEL_PATTERN_KEY);
        }

        #endregion

        #region OCarousel items

        public async virtual Task<IPagedList<OCarouselItem>> GetOCarouselItemsByOCarouselIdAsync(int carouselId, int pageIndex = 0,
            int pageSize = int.MaxValue)
        {
            var query = _carouselItemRepository.Table;

            query = query.Where(carouselItem => carouselItem.OCarouselId == carouselId)
                .OrderBy(carouselItem => carouselItem.DisplayOrder);

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        public async virtual Task<OCarouselItem> GetOCarouselItemByIdAsync(int carouselItemId)
        {
            if (carouselItemId == 0)
                return null;

            return await _carouselItemRepository.GetByIdAsync(carouselItemId, cache => default);
        }

        public async Task InsertOCarouselItemAsync(OCarouselItem carouselItem)
        {
            await _carouselItemRepository.InsertAsync(carouselItem);

            await _cacheManager.RemoveByPrefixAsync(OCAROUSEL_PATTERN_KEY);
        }

        public async virtual Task UpdateOCarouselItemAsync(OCarouselItem carouselItem)
        {
            await _carouselItemRepository.UpdateAsync(carouselItem);

            await _cacheManager.RemoveByPrefixAsync(OCAROUSEL_PATTERN_KEY);
        }

        public async virtual Task DeleteOCarouselItemAsync(OCarouselItem carouselItem)
        {
            await _carouselItemRepository.DeleteAsync(carouselItem);

            await _cacheManager.RemoveByPrefixAsync(OCAROUSEL_PATTERN_KEY);
        }

        #endregion

        #endregion
    }
}
