using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Affiliates;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Data;
using NopStation.Plugin.Widgets.AffiliateStation.Domains;
using NopStation.Plugin.Widgets.AffiliateStation.Services.Cache;

namespace NopStation.Plugin.Widgets.AffiliateStation.Services
{
    public class AffiliateCustomerService : IAffiliateCustomerService
    {
        #region Fields

        private readonly IRepository<Affiliate> _affiliateRepository;
        private readonly IRepository<Address> _addressRepository;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<AffiliateCustomer> _affiliateCustomerRepository;
        private readonly IStaticCacheManager _staticCacheManager;

        #endregion

        #region Ctor

        public AffiliateCustomerService(IRepository<Affiliate> affiliateRepository,
            IRepository<Address> addressRepository,
            IRepository<Customer> customerRepository,
            IRepository<AffiliateCustomer> affiliateCustomerRepository,
            IStaticCacheManager staticCacheManager)
        {
            _affiliateRepository = affiliateRepository;
            _addressRepository = addressRepository;
            _customerRepository = customerRepository;
            _affiliateCustomerRepository = affiliateCustomerRepository;
            _staticCacheManager = staticCacheManager;
        }

        #endregion

        #region Methods

        public async Task DeleteAffiliateCustomerAsync(AffiliateCustomer affiliateCustomer)
        {
            await _affiliateCustomerRepository.DeleteAsync(affiliateCustomer);
        }

        public async Task InsertAffiliateCustomerAsync(AffiliateCustomer affiliateCustomer)
        {
            await _affiliateCustomerRepository.InsertAsync(affiliateCustomer);
        }

        public async Task<AffiliateCustomer> GetAffiliateCustomerByCustomerIdAsync(int customerId)
        {
            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(AffiliateStationCacheDefaults.AffiliateCustomerByCustomerIdKey, customerId);

            return await _staticCacheManager.GetAsync(cacheKey, async () => 
                await _affiliateCustomerRepository.Table.FirstOrDefaultAsync(ac => ac.CustomerId == customerId));
        }

        public async Task<AffiliateCustomer> GetAffiliateCustomerByAffiliateIdAsync(int affiliateId)
        {
            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(AffiliateStationCacheDefaults.AffiliateCustomerByAffiliateIdKey, affiliateId);

            return await _staticCacheManager.GetAsync(cacheKey, async () =>
                await _affiliateCustomerRepository.Table.FirstOrDefaultAsync(ac => ac.AffiliateId == affiliateId));
        }

        public async Task<IPagedList<AffiliateCustomer>> SearchAffiliateCustomersAsync(string customerEmail = null,
            string firstName = null, string lastName = null, bool? active = null, DateTime? createdFromUtc = null,
            DateTime? createdToUtc = null, IList<int> applyStatusIds = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            applyStatusIds ??= new List<int>();

            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(AffiliateStationCacheDefaults.AffiliateCustomersAllKey,
                customerEmail, firstName, lastName, active, createdFromUtc, createdToUtc, applyStatusIds, pageIndex, pageSize);

            return await _staticCacheManager.GetAsync(cacheKey, async () =>
            {
                var query = from ac in _affiliateCustomerRepository.Table
                            join af in _affiliateRepository.Table on ac.AffiliateId equals af.Id
                            join ad in _addressRepository.Table on af.AddressId equals ad.Id
                            join c in _customerRepository.Table on ac.CustomerId equals c.Id
                            where !af.Deleted && !c.Deleted &&
                            (string.IsNullOrWhiteSpace(customerEmail) || c.Email.Contains(customerEmail)) &&
                            (string.IsNullOrWhiteSpace(firstName) || ad.FirstName.Contains(firstName)) &&
                            (string.IsNullOrWhiteSpace(lastName) || ad.LastName.Contains(lastName)) &&
                            (createdFromUtc == null || ac.CreatedOnUtc >= createdFromUtc) &&
                            (createdToUtc == null || ac.CreatedOnUtc <= createdToUtc) &&
                            (applyStatusIds.Count() == 0 || applyStatusIds.Contains(ac.ApplyStatusId))
                            select ac;

                return await query.ToPagedListAsync(pageIndex, pageSize);
            });
        }

        public async Task UpdateAffiliateCustomerAsync(AffiliateCustomer affiliateCustomer)
        {
            await _affiliateCustomerRepository.UpdateAsync(affiliateCustomer);
        }

        #endregion
    }
}