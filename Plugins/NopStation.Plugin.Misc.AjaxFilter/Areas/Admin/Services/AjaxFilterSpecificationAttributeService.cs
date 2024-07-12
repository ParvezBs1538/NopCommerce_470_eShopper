using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using NopStation.Plugin.Misc.AjaxFilter.Areas.Admin.Models;
using NopStation.Plugin.Misc.AjaxFilter.Domains;

namespace NopStation.Plugin.Misc.AjaxFilter.Areas.Admin.Services
{
    public class AjaxFilterSpecificationAttributeService : IAjaxFilterSpecificationAttributeService
    {
        private readonly IRepository<AjaxFilterSpecificationAttribute> _ajaxFilterSpecificationAttributeRepository;
        private readonly IRepository<SpecificationAttribute> _specificationAttributeRepository;
        private readonly IRepository<SpecificationAttributeOption> _specificationAttributeOptionRepository;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly IStoreContext _storeContext;

        public AjaxFilterSpecificationAttributeService(
            IRepository<AjaxFilterSpecificationAttribute> ajaxFilterSpecificationAttributeRepository,
            ISpecificationAttributeService specificationAttributeService,
            IRepository<SpecificationAttributeOption> specificationAttributeOptionRepository,
            IRepository<SpecificationAttribute> specificationAttributeRepository,
            IStaticCacheManager staticCacheManager,
            IWorkContext workContext,
            ICustomerService customerService,
            IStoreContext storeContext)
        {
            _ajaxFilterSpecificationAttributeRepository = ajaxFilterSpecificationAttributeRepository;
            _specificationAttributeService = specificationAttributeService;
            _specificationAttributeRepository = specificationAttributeRepository;
            _specificationAttributeOptionRepository = specificationAttributeOptionRepository;
            _staticCacheManager = staticCacheManager;
            _workContext = workContext;
            _customerService = customerService;
            _storeContext = storeContext;
        }

        public async Task DeleteAjaxFilterSpecificationAttribute(AjaxFilterSpecificationAttribute ajaxFilterSpecificationAttribute)
        {
            if (ajaxFilterSpecificationAttribute == null)
                throw new ArgumentNullException(nameof(ajaxFilterSpecificationAttribute));

            await _ajaxFilterSpecificationAttributeRepository.DeleteAsync(ajaxFilterSpecificationAttribute);
        }

        public async Task<IPagedList<AjaxFilterSpecificationAttribute>> GetAjaxFilterSpecificationAttributesAsync(AjaxFilterSpecificationAttributeSearchModel searchModel, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = from ajaxFilterSpecificationAttribute in _ajaxFilterSpecificationAttributeRepository.Table
                        join specificationAttribute in _specificationAttributeRepository.Table on ajaxFilterSpecificationAttribute.SpecificationId equals specificationAttribute.Id
                        where specificationAttribute.Name.Contains(searchModel.SearchSpecificationAttributeName)
                        orderby ajaxFilterSpecificationAttribute.DisplayOrder
                        select ajaxFilterSpecificationAttribute;

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        public async Task<AjaxFilterSpecificationAttribute> GetSpecificationAttributeByIdAsync(int id)
        {
            if (id == 0)
                return null;

            return await _ajaxFilterSpecificationAttributeRepository.GetByIdAsync(id);
        }

        public async Task<SpecificationAttribute> GetSpecificationAttributeBySpecificationIdAsync(int id)
        {
            return await _specificationAttributeService.GetSpecificationAttributeByIdAsync(id);
        }

        public async Task<AjaxFilterSpecificationAttribute> GetSpecificationAttributeByNameAsync(string name)
        {
            if (name == null)
                name = string.Empty;
            name = name.Trim().ToLowerInvariant();

            var query = from l in _ajaxFilterSpecificationAttributeRepository.Table
                        join x in _specificationAttributeRepository.Table on l.SpecificationId equals x.Id
                        orderby x.Name
                        where x.Name.ToLower() == name
                        select l;

            return await query.FirstOrDefaultAsync();
        }

        public async Task InsertAjaxFilterSpecificationAttributeAsync(AjaxFilterSpecificationAttribute ajaxFilterSpecificationAttribute)
        {
            if (ajaxFilterSpecificationAttribute == null)
                throw new ArgumentNullException(nameof(ajaxFilterSpecificationAttribute));

            await _ajaxFilterSpecificationAttributeRepository.InsertAsync(ajaxFilterSpecificationAttribute);
        }

        public async Task<IPagedList<SpecificationAttribute>> GetSpecificationAttributesAsync(AjaxFilterSpecificationAttributeSearchModel searchModel, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = from sa in _specificationAttributeRepository.Table
                        where sa.Name.Contains(searchModel.SearchSpecificationAttributeName)
                        orderby sa.DisplayOrder, sa.Id
                        select sa;

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        public async Task<IList<AjaxFilterSpecificationAttribute>> GetAvailableSpecificationAttributesAsync()
        {
            var query = _ajaxFilterSpecificationAttributeRepository.Table;

            var storeId = (await _storeContext.GetCurrentStoreAsync()).Id;

            var customer = await _workContext.GetCurrentCustomerAsync();
            var customerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer);

            var cacheKey = _staticCacheManager
                .PrepareKeyForDefaultCache(AjaxFilterDefaults.AjaxFilterAvailableSpecificationAttributesCacheKey, customerRoleIds, storeId);

            return await _staticCacheManager.GetAsync(cacheKey, () => query.OrderBy(x => x.DisplayOrder).ToList());
        }

        public async Task<AjaxFilterSpecificationAttribute> GetAjaxFilterSpecificationAttributeBySpecificationAttributeId(int id)
        {
            var query = from ajaxFilterSpecificationAttribute in _ajaxFilterSpecificationAttributeRepository.Table
                        where ajaxFilterSpecificationAttribute.SpecificationId == id
                        select ajaxFilterSpecificationAttribute;

            return await query.FirstOrDefaultAsync();
        }

        public async Task<List<SpecificationAttributeOption>> GetSpecificationAttributeOptionBySpecificationName(string name)
        {
            var query = from sao in _specificationAttributeOptionRepository.Table
                        join sa in _specificationAttributeRepository.Table on sao.SpecificationAttributeId equals sa.Id
                        where sa.Name.Equals(name)
                        select sao;

            return await query.ToListAsync();
        }

        public async Task UpdateAjaxFilterSpecificationAttribute(AjaxFilterSpecificationAttribute ajaxFilterSpecificationAttribute)
        {
            await _ajaxFilterSpecificationAttributeRepository.UpdateAsync(ajaxFilterSpecificationAttribute);
        }

    }
}
