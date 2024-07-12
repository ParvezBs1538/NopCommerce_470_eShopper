using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Stores;
using Nop.Data;
using Nop.Services.Customers;

namespace NopStation.Plugin.Widgets.MegaMenu.Services;

public class MegaMenuCoreService : IMegaMenuCoreService
{
    #region Fields

    private readonly CatalogSettings _catalogSettings;
    private readonly IRepository<AclRecord> _aclRepository;
    private readonly IRepository<Manufacturer> _manufacturerRepository;
    private readonly IRepository<Category> _categoryRepository;
    private readonly IRepository<StoreMapping> _storeMappingRepository;
    private readonly IStoreContext _storeContext;
    private readonly IWorkContext _workContext;
    private readonly ICustomerService _customerService;

    #endregion

    #region Ctor

    public MegaMenuCoreService(CatalogSettings catalogSettings,
        IRepository<AclRecord> aclRepository,
        IRepository<Manufacturer> manufacturerRepository,
        IRepository<Category> categoryRepository,
        IRepository<StoreMapping> storeMappingRepository,
        IStoreContext storeContext,
        IWorkContext workContext,
        ICustomerService customerService
        )
    {
        _catalogSettings = catalogSettings;
        _aclRepository = aclRepository;
        _manufacturerRepository = manufacturerRepository;
        _categoryRepository = categoryRepository;
        _storeMappingRepository = storeMappingRepository;
        _storeContext = storeContext;
        _workContext = workContext;
        _customerService = customerService;
    }

    #endregion

    #region Methods

    public virtual async Task<IList<Category>> GetCategoriesByIdsAsync(int storeId = 0, List<int> selectedIds = null, int pageSize = int.MaxValue, bool showHidden = false)
    {
        var query = _categoryRepository.Table;
        if (!showHidden)
            query = query.Where(m => m.Published);
        query = query.Where(m => !m.Deleted);

        if (selectedIds != null && selectedIds.Count > 0)
            query = query.Where(e => selectedIds.Contains(e.Id));

        query = query.OrderBy(m => m.DisplayOrder).ThenBy(m => m.Id);

        if ((storeId <= 0 || _catalogSettings.IgnoreStoreLimitations) && (showHidden || _catalogSettings.IgnoreAcl))
        {
            query = pageSize == 0 ? query : query.Take(pageSize);
            return query.ToList();
        }

        if (!showHidden && !_catalogSettings.IgnoreAcl)
        {
            //ACL (access control list)
            var allowedCustomerRolesIds = await _customerService.GetCustomerRoleIdsAsync(await _workContext.GetCurrentCustomerAsync());
            query = from m in query
                    join acl in _aclRepository.Table
                        on new { c1 = m.Id, c2 = typeof(Category).Name } equals new { c1 = acl.EntityId, c2 = acl.EntityName } into m_acl
                    from acl in m_acl.DefaultIfEmpty()
                    where !m.SubjectToAcl || allowedCustomerRolesIds.Contains(acl.CustomerRoleId)
                    select m;
        }

        if (storeId > 0 && !_catalogSettings.IgnoreStoreLimitations)
        {
            //Store mapping
            query = from m in query
                    join sm in _storeMappingRepository.Table
                        on new { c1 = m.Id, c2 = typeof(Category).Name } equals new { c1 = sm.EntityId, c2 = sm.EntityName } into m_sm
                    from sm in m_sm.DefaultIfEmpty()
                    where !m.LimitedToStores || storeId == sm.StoreId
                    select m;
        }

        query = query.Distinct().OrderBy(m => m.DisplayOrder).ThenBy(m => m.Id);
        query = pageSize == 0 ? query : query.Take(pageSize);

        return query.ToList();
    }

    public virtual async Task<IList<Manufacturer>> GetManufacturersByIdsAsync(int storeId = 0, List<int> selectedIds = null, int pageSize = int.MaxValue, bool showHidden = false)
    {
        var query = _manufacturerRepository.Table;
        if (!showHidden)
            query = query.Where(m => m.Published);
        query = query.Where(m => !m.Deleted);

        if (selectedIds != null && selectedIds.Count > 0)
            query = query.Where(e => selectedIds.Contains(e.Id));

        query = query.OrderBy(m => m.DisplayOrder).ThenBy(m => m.Id);

        if ((storeId <= 0 || _catalogSettings.IgnoreStoreLimitations) && (showHidden || _catalogSettings.IgnoreAcl))
        {
            query = pageSize == 0 ? query : query.Take(pageSize);
            return query.ToList();
        }

        if (!showHidden && !_catalogSettings.IgnoreAcl)
        {
            //ACL (access control list)
            var allowedCustomerRolesIds = await _customerService.GetCustomerRoleIdsAsync(await _workContext.GetCurrentCustomerAsync());
            query = from m in query
                    join acl in _aclRepository.Table
                        on new { c1 = m.Id, c2 = typeof(Manufacturer).Name } equals new { c1 = acl.EntityId, c2 = acl.EntityName } into m_acl
                    from acl in m_acl.DefaultIfEmpty()
                    where !m.SubjectToAcl || allowedCustomerRolesIds.Contains(acl.CustomerRoleId)
                    select m;
        }

        if (storeId > 0 && !_catalogSettings.IgnoreStoreLimitations)
        {
            //Store mapping
            query = from m in query
                    join sm in _storeMappingRepository.Table
                        on new { c1 = m.Id, c2 = typeof(Manufacturer).Name } equals new { c1 = sm.EntityId, c2 = sm.EntityName } into m_sm
                    from sm in m_sm.DefaultIfEmpty()
                    where !m.LimitedToStores || storeId == sm.StoreId
                    select m;
        }

        query = query.Distinct().OrderBy(m => m.DisplayOrder).ThenBy(m => m.Id);
        query = pageSize == 0 ? query : query.Take(pageSize);

        return query.ToList();
    }

    #endregion
}
