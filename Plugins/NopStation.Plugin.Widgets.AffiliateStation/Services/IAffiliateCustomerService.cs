using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Widgets.AffiliateStation.Domains;

namespace NopStation.Plugin.Widgets.AffiliateStation.Services
{
    public interface IAffiliateCustomerService
    {
        Task DeleteAffiliateCustomerAsync(AffiliateCustomer affiliateCustomer);

        Task InsertAffiliateCustomerAsync(AffiliateCustomer affiliateCustomer);

        Task UpdateAffiliateCustomerAsync(AffiliateCustomer affiliateCustomer);

        Task<AffiliateCustomer> GetAffiliateCustomerByCustomerIdAsync(int customerId);

        Task<AffiliateCustomer> GetAffiliateCustomerByAffiliateIdAsync(int affiliateId);

        Task<IPagedList<AffiliateCustomer>> SearchAffiliateCustomersAsync(string customerEmail = null,
            string firstName = null, string lastName = null, bool? active = null, DateTime? createdFromUtc = null,
            DateTime? createdToUtc = null, IList<int> applyStatusIds = null, int pageIndex = 0, int pageSize = int.MaxValue);
    }
}