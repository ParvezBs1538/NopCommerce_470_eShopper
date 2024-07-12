using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Data;
using Nop.Services.Security;
using Nop.Services.Stores;
using NopStation.Plugin.Misc.Core.Caching;
using NopStation.Plugin.Misc.WidgetManager.Services;
using NopStation.Plugin.Widgets.Announcement.Domains;
using NopStation.Plugin.Widgets.Announcement.Services.Cache;

namespace NopStation.Plugin.Widgets.Announcement.Services;

public class AnnouncementItemService : IAnnouncementItemService
{
    #region Fields

    private readonly IWorkContext _workContext;
    private readonly IRepository<AnnouncementItem> _announcementItemRepository;
    private readonly IStoreMappingService _storeMappingService;
    private readonly IStaticCacheManager _staticCacheManager;
    private readonly IScheduleService _scheduleService;
    private readonly IAclService _aclService;
    private readonly IConditionService _conditionService;

    #endregion

    #region Ctor

    public AnnouncementItemService(IWorkContext workContext,
        IRepository<AnnouncementItem> announcementItemRepository,
        IStoreMappingService storeMappingService,
        IStaticCacheManager staticCacheManager,
        IScheduleService scheduleService,
        IAclService aclService,
        IConditionService conditionService)
    {
        _workContext = workContext;
        _announcementItemRepository = announcementItemRepository;
        _storeMappingService = storeMappingService;
        _staticCacheManager = staticCacheManager;
        _scheduleService = scheduleService;
        _aclService = aclService;
        _conditionService = conditionService;
    }

    #endregion

    #region Methods

    public async Task DeleteAnnouncementItemAsync(AnnouncementItem announcementItem)
    {
        await _announcementItemRepository.DeleteAsync(announcementItem);
    }

    public async Task InsertAnnouncementItemAsync(AnnouncementItem announcementItem)
    {
        await _announcementItemRepository.InsertAsync(announcementItem);
    }

    public async Task UpdateAnnouncementItemAsync(AnnouncementItem announcementItem)
    {
        await _announcementItemRepository.UpdateAsync(announcementItem);
    }

    public async Task<AnnouncementItem> GetAnnouncementItemByIdAsync(int announcementItemId)
    {
        if (announcementItemId == 0)
            return null;

        return await _announcementItemRepository.GetByIdAsync(announcementItemId, cache =>
            _staticCacheManager.PrepareKeyForDefaultCache(NopStationEntityCacheDefaults<AnnouncementItem>.ByIdCacheKey, announcementItemId));
    }

    public async Task<IList<AnnouncementItem>> GetAllAnnouncementItemsAsync(string keywords = null, bool showHidden = false, bool? overridePublished = false,
        bool validScheduleOnly = false, int storeId = 0)
    {
        var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(AnnouncementCacheDefaults.AnnouncementItemsAllKey,
            keywords, showHidden, overridePublished, storeId, validScheduleOnly);

        var query = _announcementItemRepository.Table;

        if (!showHidden)
            query = query.Where(s => s.Active);
        else if (overridePublished.HasValue)
            query = query.Where(s => s.Active == overridePublished.Value);

        if (!showHidden)
        {
            //apply ACL constraints
            var customer = await _workContext.GetCurrentCustomerAsync();
            query = await _aclService.ApplyAcl(query, customer);

            //apply customer condition mapping constraints
            query = await _conditionService.ApplyCustomerConditionMappingAsync(query, customer.Id);
        }

        if (!string.IsNullOrWhiteSpace(keywords))
            query = query.Where(s => s.Name.Contains(keywords) || s.Title.Contains(keywords) || s.Description.Contains(keywords));

        //apply store mapping constraints
        query = await _storeMappingService.ApplyStoreMapping(query, storeId);

        query = query.OrderBy(x => x.DisplayOrder);

        var items = await query.ToListAsync();

        if (validScheduleOnly)
            items = await items.WhereAwait(s => new ValueTask<bool>(_scheduleService.IsValidByDateTime(s))).ToListAsync();

        return items;
    }

    #endregion
}
