using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Data;
using Nop.Services.Security;
using Nop.Services.Stores;
using NopStation.Plugin.Misc.WidgetManager.Services;
using NopStation.Plugin.Widgets.Popups.Domains;

namespace NopStation.Plugin.Widgets.Popups.Services;

public class PopupService : IPopupService
{
    #region Fields

    private readonly IStaticCacheManager _cacheManager;
    private readonly IRepository<Popup> _popupRepository;
    private readonly IStoreMappingService _storeMappingService;
    private readonly IWorkContext _workContext;
    private readonly IAclService _aclService;
    private readonly IScheduleService _scheduleService;
    private readonly IConditionService _conditionService;

    #endregion

    #region Ctor

    public PopupService(IRepository<Popup> popupRepository,
        IStaticCacheManager cacheManager,
        IStoreMappingService storeMappingService,
        IWorkContext workContext,
        IAclService aclService,
        IScheduleService scheduleService,
        IConditionService conditionService)
    {
        _popupRepository = popupRepository;
        _storeMappingService = storeMappingService;
        _workContext = workContext;
        _aclService = aclService;
        _scheduleService = scheduleService;
        _conditionService = conditionService;
        _cacheManager = cacheManager;
    }

    #endregion

    #region Methods

    public virtual async Task<IPagedList<Popup>> GetAllPopupsAsync(string keywords = null, int storeId = 0, int productId = 0,
        bool overrideProduct = false, bool showHidden = false, bool? overridePublished = null,
        bool validScheduleOnly = false, DeviceType? deviceType = null, int pageIndex = 0, int pageSize = int.MaxValue)
    {
        var query = _popupRepository.Table.Where(c => !c.Deleted);

        if (!showHidden)
            query = query.Where(s => s.Active);
        else if (overridePublished.HasValue)
            query = query.Where(s => s.Active == overridePublished.Value);

        if (deviceType.HasValue)
        {
            if (deviceType.Value == DeviceType.Mobile)
                query = query.Where(s => s.DeviceTypeId == (int)DeviceType.Mobile || s.DeviceTypeId == (int)DeviceType.Both);
            if (deviceType.Value == DeviceType.Desktop)
                query = query.Where(s => s.DeviceTypeId == (int)DeviceType.Desktop || s.DeviceTypeId == (int)DeviceType.Both);
        }

        //apply store mapping constraints
        query = await _storeMappingService.ApplyStoreMapping(query, storeId);

        //apply product condition mapping constraints
        if (overrideProduct)
            query = await _conditionService.ApplyProductConditionMappingAsync(query, productId);

        if (!showHidden)
        {
            //apply ACL constraints
            var customer = await _workContext.GetCurrentCustomerAsync();
            query = await _aclService.ApplyAcl(query, customer);

            //apply customer condition mapping constraints
            query = await _conditionService.ApplyCustomerConditionMappingAsync(query, customer.Id);
        }

        if (!string.IsNullOrWhiteSpace(keywords))
            query = query.Where(s => s.Name.Contains(keywords) || s.StickyButtonText.Contains(keywords) ||
                s.Column1Text.Contains(keywords) || s.Column2Text.Contains(keywords));

        //apply schedule mapping constraints
        if (validScheduleOnly)
            query = await _scheduleService.ApplyScheduleMappingAsync(query);

        query = query.OrderByDescending(c => c.Id);

        return await query.ToPagedListAsync(pageIndex, pageSize);
    }

    public virtual async Task<Popup> GetPopupByIdAsync(int popupId)
    {
        if (popupId == 0)
            return null;

        return await _popupRepository.GetByIdAsync(popupId, cache => default);
    }

    public virtual async Task InsertPopupAsync(Popup popup)
    {
        await _popupRepository.InsertAsync(popup);
    }

    public virtual async Task UpdatePopupAsync(Popup popup)
    {
        await _popupRepository.UpdateAsync(popup);
    }

    public virtual async Task DeletePopupAsync(Popup popup)
    {
        await _popupRepository.DeleteAsync(popup);
    }

    #endregion
}
