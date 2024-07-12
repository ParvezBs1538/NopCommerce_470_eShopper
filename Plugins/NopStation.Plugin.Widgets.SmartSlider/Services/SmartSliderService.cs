using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Data;
using Nop.Services.Security;
using Nop.Services.Stores;
using NopStation.Plugin.Misc.WidgetManager.Services;
using NopStation.Plugin.Widgets.SmartSliders.Domains;
using NopStation.Plugin.Widgets.SmartSliders.Services.Cache;

namespace NopStation.Plugin.Widgets.SmartSliders.Services;

public class SmartSliderService : ISmartSliderService
{
    #region Fields

    private readonly IStoreMappingService _storeMappingService;
    private readonly IRepository<SmartSlider> _sliderRepository;
    private readonly IRepository<SmartSliderItem> _sliderItemRepository;
    private readonly IConditionService _conditionService;
    private readonly IWorkContext _workContext;
    private readonly IAclService _aclService;
    private readonly IScheduleService _scheduleService;
    private readonly IWidgetZoneService _widgetZoneService;
    private readonly IStaticCacheManager _staticCacheManager;

    #endregion

    #region Ctor

    public SmartSliderService(IStoreMappingService storeMappingService,
        IRepository<SmartSlider> sliderRepository,
        IRepository<SmartSliderItem> sliderItemRepository,
        IConditionService conditionService,
        IWorkContext workContext,
        IAclService aclService,
        IScheduleService scheduleService,
        IWidgetZoneService widgetZoneService,
        IStaticCacheManager staticCacheManager)
    {
        _storeMappingService = storeMappingService;
        _sliderRepository = sliderRepository;
        _sliderItemRepository = sliderItemRepository;
        _conditionService = conditionService;
        _workContext = workContext;
        _aclService = aclService;
        _scheduleService = scheduleService;
        _widgetZoneService = widgetZoneService;
        _staticCacheManager = staticCacheManager;
    }

    #endregion

    #region Methods

    #region Sliders

    public virtual async Task<IPagedList<SmartSlider>> GetAllSlidersAsync(string keywords = null, int storeId = 0,
        int productId = 0, bool overrideProduct = false, bool showHidden = false, bool? overridePublished = null,
        bool validScheduleOnly = false, string widgetZone = null, int pageIndex = 0, int pageSize = int.MaxValue)
    {
        var query = _sliderRepository.Table.Where(c => !c.Deleted);

        if (!showHidden)
            query = query.Where(s => s.Active);
        else if (overridePublished.HasValue)
            query = query.Where(s => s.Active == overridePublished.Value);

        //apply store mapping constraints
        query = await _storeMappingService.ApplyStoreMapping(query, storeId);

        //apply product condition mapping constraints
        if (overrideProduct)
            query = await _conditionService.ApplyProductConditionMappingAsync(query, productId);

        //apply widget zone mapping constraints
        query = await _widgetZoneService.ApplyWidgetZoneMappingAsync(query, widgetZone);

        if (!showHidden)
        {
            //apply ACL constraints
            var customer = await _workContext.GetCurrentCustomerAsync();
            query = await _aclService.ApplyAcl(query, customer);

            //apply customer condition mapping constraints
            query = await _conditionService.ApplyCustomerConditionMappingAsync(query, customer.Id);
        }

        if (!string.IsNullOrWhiteSpace(keywords))
            query = query.Where(s => s.Name.Contains(keywords));

        //apply schedule mapping constraints
        if (validScheduleOnly)
            query = await _scheduleService.ApplyScheduleMappingAsync(query);

        query = query.OrderBy(c => c.DisplayOrder);

        var data = await query.ToPagedListAsync(pageIndex, pageSize);

        return await Task.FromResult(data);
    }

    public virtual async Task<SmartSlider> GetSliderByIdAsync(int sliderId)
    {
        return await _sliderRepository.GetByIdAsync(sliderId, cache => default);
    }

    public virtual async Task InsertSliderAsync(SmartSlider slider)
    {
        await _sliderRepository.InsertAsync(slider);
    }

    public virtual async Task UpdateSliderAsync(SmartSlider slider)
    {
        await _sliderRepository.UpdateAsync(slider);
    }

    public virtual async Task DeleteSliderAsync(SmartSlider slider, bool deleteReletedData = true)
    {
        if (deleteReletedData)
        {
            var widgetZoneMappings = await _widgetZoneService.GetEntityWidgetZoneMappingsAsync(slider);
            await _widgetZoneService.DeleteWidgetZoneMappingsAsync(widgetZoneMappings);

            var customerConditions = await _conditionService.GetEntityCustomerConditionsAsync(slider);
            await _conditionService.DeleteCustomerConditionMappingsAsync(customerConditions);

            var productConditions = await _conditionService.GetEntityProductConditionsAsync(slider);
            await _conditionService.DeleteProductConditionMappingsAsync(productConditions);
        }

        await _sliderRepository.DeleteAsync(slider);
    }

    #endregion

    #region Slider items

    public virtual async Task<IList<SmartSliderItem>> GetSliderItemsBySliderIdAsync(int sliderId, int languageId = 0, int recordsToReturn = 0)
    {
        var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(CacheDefaults.SliderItemsBySliderIdKey, sliderId, languageId, recordsToReturn);

        return await _staticCacheManager.GetAsync(cacheKey, async () =>
        {
            var query = from si in _sliderItemRepository.Table
                        where si.SliderId == sliderId &&
                            (languageId == 0 || si.LanguageId == 0 || si.LanguageId == languageId)
                        select si;

            if (recordsToReturn > 0)
                query = query.Take(recordsToReturn);

            query = query.OrderBy(si => si.DisplayOrder);

            return await query.ToListAsync();
        });
    }

    public virtual async Task<SmartSliderItem> GetSliderItemByIdAsync(int sliderItemId)
    {
        if (sliderItemId == 0)
            return null;

        return await _sliderItemRepository.GetByIdAsync(sliderItemId, cache => default);
    }

    public async Task InsertSliderItemAsync(SmartSliderItem sliderItem)
    {
        await _sliderItemRepository.InsertAsync(sliderItem);
    }

    public virtual async Task UpdateSliderItemAsync(SmartSliderItem sliderItem)
    {
        await _sliderItemRepository.UpdateAsync(sliderItem);
    }

    public virtual async Task DeleteSliderItemAsync(SmartSliderItem sliderItem)
    {
        await _sliderItemRepository.DeleteAsync(sliderItem);
    }

    #endregion

    #endregion
}
