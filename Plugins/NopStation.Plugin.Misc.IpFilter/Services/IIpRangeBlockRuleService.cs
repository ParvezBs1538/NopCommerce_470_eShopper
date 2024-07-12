using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firewall;
using Nop.Core;
using NopStation.Plugin.Misc.IpFilter.Domain;

namespace NopStation.Plugin.Misc.IpFilter.Services
{
    public interface IIpRangeBlockRuleService
    {
        Task<IpRangeBlockRule> GetIpRangeBlockRuleByIdAsync(int id);

        Task<IList<CIDRNotation>> GetAllCIDRNotationsAsync();

        Task<IPagedList<IpRangeBlockRule>> GetIpRangeBlockRulesAsync(DateTime? createdFromUtc = null,
            DateTime? createdToUtc = null, int pageIndex = 0, int pageSize = int.MaxValue);

        Task InsertIpRangeBlockRuleAsync(IpRangeBlockRule ipRangeBlockRule);

        Task UpdateIpRangeBlockRuleAsync(IpRangeBlockRule ipRangeBlockRule);

        Task DeleteIpRangeBlockRuleAsync(IpRangeBlockRule ipRangeBlockRule);
    }
}
