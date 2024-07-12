using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Misc.IpFilter.Domain;

namespace NopStation.Plugin.Misc.IpFilter.Services
{
    public interface IIpBlockRuleService
    {
        Task<IpBlockRule> GetIpBlockRuleByIdAsync(int id);

        Task<IList<IPAddress>> GetAllBlockedIPAddressesAsync();

        Task<IPagedList<IpBlockRule>> GetIpBlockRulesAsync(DateTime? createdFromUtc = null,
            DateTime? createdToUtc = null, int pageIndex = 0, int pageSize = int.MaxValue);

        Task InsertIpBlockRuleAsync(IpBlockRule ipBlockRule);

        Task UpdateIpBlockRuleAsync(IpBlockRule ipBlockRule);

        Task DeleteIpBlockRuleAsync(IpBlockRule ipBlockRule);
    }
}
