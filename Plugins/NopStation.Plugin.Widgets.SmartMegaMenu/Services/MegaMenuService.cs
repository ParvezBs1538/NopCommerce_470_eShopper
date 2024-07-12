using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Services.Topics;
using Nop.Services.Vendors;
using NopStation.Plugin.Misc.WidgetManager.Services;
using NopStation.Plugin.Widgets.SmartMegaMenu.Domain;
using NopStation.Plugin.Widgets.SmartMegaMenu.Services.Cache;

namespace NopStation.Plugin.Widgets.SmartMegaMenu.Services;

public class MegaMenuService : IMegaMenuService
{
    #region Fields

    private readonly IRepository<MegaMenu> _megaMenuRepository;
    private readonly IRepository<MegaMenuItem> _megaMenuItemRepository;
    private readonly IStoreMappingService _storeMappingService;
    private readonly IWidgetZoneService _widgetZoneService;
    private readonly IWorkContext _workContext;
    private readonly IAclService _aclService;
    private readonly IStaticCacheManager _staticCacheManager;
    private readonly ICustomerService _customerService;
    private readonly ICategoryService _categoryService;
    private readonly IManufacturerService _manufacturerService;
    private readonly IVendorService _vendorService;
    private readonly IProductTagService _productTagService;
    private readonly ITopicService _topicService;

    #endregion

    #region Ctor

    public MegaMenuService(IRepository<MegaMenu> megaMenuRepository,
        IRepository<MegaMenuItem> megaMenuItemRepository,
        IStoreMappingService storeMappingService,
        IWidgetZoneService widgetZoneService,
        IWorkContext workContext,
        IAclService aclService,
        IStaticCacheManager staticCacheManager,
        ICustomerService customerService,
        ICategoryService categoryService,
        IManufacturerService manufacturerService,
        IVendorService vendorService,
        IProductTagService productTagService,
        ITopicService topicService)
    {
        _megaMenuRepository = megaMenuRepository;
        _megaMenuItemRepository = megaMenuItemRepository;
        _storeMappingService = storeMappingService;
        _widgetZoneService = widgetZoneService;
        _workContext = workContext;
        _aclService = aclService;
        _staticCacheManager = staticCacheManager;
        _customerService = customerService;
        _categoryService = categoryService;
        _manufacturerService = manufacturerService;
        _vendorService = vendorService;
        _productTagService = productTagService;
        _topicService = topicService;
    }

    #endregion

    #region Utilities

    protected async Task<List<MegaMenuItem>> GetValidMenuItemsAsync(List<MegaMenuItem> menuItems)
    {
        var validMenuItems = new List<MegaMenuItem>();
        foreach (var item in menuItems)
        {
            switch (item.MenuItemType)
            {
                case MenuItemType.Category:
                    var category = await _categoryService.GetCategoryByIdAsync(item.CategoryId);
                    if (category == null || category.Deleted)
                        continue;
                    break;
                case MenuItemType.Manufacturer:
                    var manufacturer = await _manufacturerService.GetManufacturerByIdAsync(item.ManufacturerId);
                    if (manufacturer == null || manufacturer.Deleted)
                        continue;
                    break;
                case MenuItemType.Vendor:
                    var vendor = await _vendorService.GetVendorByIdAsync(item.VendorId);
                    if (vendor == null || vendor.Deleted)
                        continue;
                    break;
                case MenuItemType.Topic:
                    var topic = await _topicService.GetTopicByIdAsync(item.TopicId);
                    if (topic == null)
                        continue;

                    break;
                case MenuItemType.ProductTag:
                    var tag = await _productTagService.GetProductTagByIdAsync(item.ProductTagId);
                    if (tag == null)
                        continue;
                    break;
                case MenuItemType.Page:
                case MenuItemType.CustomLink:
                default:
                    break;
            }

            validMenuItems.Add(item);
        }

        return validMenuItems;
    }

    #endregion

    #region Methods

    #region Mega menus

    public async Task InsertMegaMenuAsync(MegaMenu megaMenu)
    {
        await _megaMenuRepository.InsertAsync(megaMenu);
    }

    public async Task UpdateMegaMenuAsync(MegaMenu megaMenu)
    {
        await _megaMenuRepository.UpdateAsync(megaMenu);
    }

    public async Task DeleteMegaMenuAsync(MegaMenu megaMenu)
    {
        await _megaMenuRepository.DeleteAsync(megaMenu);
    }

    public async Task<MegaMenu> GetMegaMenuByIdAsync(int megaMenuId)
    {
        return await _megaMenuRepository.GetByIdAsync(megaMenuId, includeDeleted: false);
    }

    public async Task<IPagedList<MegaMenu>> GetAllMegaMenusAsync(string keyword = null, int storeId = 0, string widgetZone = null,
        bool showHidden = false, bool? overridePublished = null, int pageIndex = 0, int pageSize = int.MaxValue)
    {
        var query = _megaMenuRepository.Table.Where(c => !c.Deleted);

        if (!showHidden)
            query = query.Where(mm => mm.Active);
        else if (overridePublished.HasValue)
            query = query.Where(mm => mm.Active == overridePublished.Value);

        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(mm => mm.Name.Contains(keyword));

        //apply widget zone mapping constraints
        query = await _widgetZoneService.ApplyWidgetZoneMappingAsync(query, widgetZone);

        //apply store mapping constraints
        query = await _storeMappingService.ApplyStoreMapping(query, storeId);

        query = query.OrderBy(mm => mm.DisplayOrder);

        return await query.ToPagedListAsync(pageIndex, pageSize);
    }

    #endregion

    #region Mega menu Items

    public virtual async Task<MegaMenuItem> GetMegaMenuItemByIdAsync(int megaMenuItemId)
    {
        return await _megaMenuItemRepository.GetByIdAsync(megaMenuItemId);
    }

    public virtual async Task<IList<MegaMenuItem>> GetMegaMenuItemsByIdAsync(int[] megaMenuItemIds)
    {
        return await _megaMenuItemRepository.GetByIdsAsync(megaMenuItemIds, includeDeleted: false);
    }

    public virtual async Task UpdateMegaMenuItemAsync(MegaMenuItem megaMenuItem)
    {
        await _megaMenuItemRepository.UpdateAsync(megaMenuItem);
    }

    public virtual async Task UpdateMegaMenuItemsAsync(IList<MegaMenuItem> megaMenuItems)
    {
        await _megaMenuItemRepository.UpdateAsync(megaMenuItems);
    }

    public virtual async Task InsertMegaMenuItemAsync(MegaMenuItem megaMenuItem)
    {
        await _megaMenuItemRepository.InsertAsync(megaMenuItem);
    }

    public virtual async Task DeleteMegaMenuItemAsync(MegaMenuItem megaMenuItem)
    {
        await _megaMenuItemRepository.DeleteAsync(megaMenuItem);
    }

    public virtual async Task DeleteMegaMenuItemsAsync(IList<MegaMenuItem> megaMenuItems)
    {
        await _megaMenuItemRepository.DeleteAsync(megaMenuItems);
    }

    public async Task<IList<MegaMenuItem>> GetAllMegaMenuItemsAsync(int megaMenuId, MenuItemType? menuItemType = null, bool showHidden = false)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();

        var cacheKey = _staticCacheManager.PrepareKey(CacheDefaults.MegaMenuItemsKey, megaMenuId,
            menuItemType.HasValue ? (int?)menuItemType.Value : null,
            await _customerService.GetCustomerRolesAsync(customer));

        var menuItems = await _staticCacheManager.GetAsync(cacheKey, async () =>
        {
            var query = _megaMenuItemRepository.Table.Where(mi => mi.MegaMenuId == megaMenuId);

            if (menuItemType != null)
                query = query.Where(mi => mi.MenuItemTypeId == (int)menuItemType.Value);

            //apply ACL constraints
            if (!showHidden)
                query = await _aclService.ApplyAcl(query, customer);

            query = query.OrderBy(mi => mi.DisplayOrder);

            return await query.ToListAsync();
        });

        var validMenuItems = await GetValidMenuItemsAsync(menuItems);

        return validMenuItems;
    }

    public async Task<IList<MegaMenuItem>> GetMegaMenuItemsByParentMenuItemIdAsync(int parentMenuItemId, bool showHidden = false)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();

        var cacheKey = _staticCacheManager.PrepareKey(CacheDefaults.MegaMenuItemsByParentIdKey, parentMenuItemId,
            await _customerService.GetCustomerRolesAsync(customer));

        var menuItems = await _staticCacheManager.GetAsync(cacheKey, async () =>
        {
            var query = _megaMenuItemRepository.Table.Where(mi => mi.ParentMenuItemId == parentMenuItemId);

            //apply ACL constraints
            if (!showHidden)
                query = await _aclService.ApplyAcl(query, customer);

            query = query.OrderBy(mi => mi.DisplayOrder);

            return await query.ToListAsync();
        });

        var validMenuItems = await GetValidMenuItemsAsync(menuItems);

        return validMenuItems;
    }

    #endregion

    #endregion
}
