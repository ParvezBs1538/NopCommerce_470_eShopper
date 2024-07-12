using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Data;
using NopStation.Plugin.Misc.WebApi.Domains;
using NopStation.Plugin.Misc.WebApi.Infrastructure.Cache;

namespace NopStation.Plugin.Misc.WebApi.Services
{
    public class ApiSliderService : IApiSliderService
    {
        #region Fields

        private readonly IRepository<ApiSlider> _sliderRepository;
        private readonly IStaticCacheManager _cacheManager;

        #endregion

        #region Ctor

        public ApiSliderService(IRepository<ApiSlider> sliderRepository,
            IStaticCacheManager cacheManager)
        {
            _sliderRepository = sliderRepository;
            _cacheManager = cacheManager;
        }

        #endregion

        #region Methods

        public async Task DeleteApiSliderAsync(ApiSlider slider)
        {
            await _sliderRepository.DeleteAsync(slider);

            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.SliderPrefixCacheKey);
        }

        public async Task InsertApiSliderAsync(ApiSlider slider)
        {
            await _sliderRepository.InsertAsync(slider);

            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.SliderPrefixCacheKey);
        }

        public async Task InsertApiSliderAsync(List<ApiSlider> sliders)
        {
            await _sliderRepository.InsertAsync(sliders);

            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.SliderPrefixCacheKey);
        }

        public async Task UpdateApiSliderAsync(ApiSlider slider)
        {
            await _sliderRepository.UpdateAsync(slider);

            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.SliderPrefixCacheKey);
        }

        public async Task<ApiSlider> GetApiSliderByIdAsync(int sliderId)
        {
            if (sliderId == 0)
                return null;

            return await _sliderRepository.GetByIdAsync(sliderId, cache => default);
        }

        public async Task<IPagedList<ApiSlider>> GetAllApiSlidersAsync(List<int> stids = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var sliders = _sliderRepository.Table;

            if (stids != null && stids.Any())
                sliders = sliders.Where(x => stids.Contains(x.SliderTypeId));

            sliders = sliders.OrderBy(e => e.DisplayOrder);

            return await sliders.ToPagedListAsync(pageIndex, pageSize);
        }

        public async Task<IList<ApiSlider>> GetActiveApiSlidersAsync(int maximumItems = 0)
        {
            var sliders = _sliderRepository.Table
                .Where(x => (!x.ActiveStartDateUtc.HasValue || x.ActiveStartDateUtc <= DateTime.UtcNow) &&
                (!x.ActiveEndDateUtc.HasValue || x.ActiveEndDateUtc >= DateTime.UtcNow));

            sliders = sliders.OrderBy(e => e.DisplayOrder);

            return await sliders.ToPagedListAsync(0, maximumItems);
        }

        public IList<ApiSlider> GetApiSliderByIds(int[] ids)
        {
            if (ids == null || ids.Length == 0)
                return new List<ApiSlider>();

            var query = from s in _sliderRepository.Table
                        where ids.Contains(s.Id)
                        select s;
            var logItems = query.ToList();

            //sort by passed identifiers
            var sortedLogItems = new List<ApiSlider>();
            foreach (var id in ids)
            {
                var slider = logItems.Find(x => x.Id == id);
                if (slider != null)
                    sortedLogItems.Add(slider);
            }
            return sortedLogItems;
        }

        public async Task DeleteApiSlidersAsync(IList<ApiSlider> sliders)
        {
            await _sliderRepository.DeleteAsync(sliders);

            await _cacheManager.RemoveByPrefixAsync(ApiModelCacheDefaults.SliderPrefixCacheKey);
        }

        #endregion
    }
}
