using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using NopStation.Plugin.Misc.IpFilter.Domain;

namespace NopStation.Plugin.Misc.IpFilter.Services
{
    public class IpBlockRuleService : IIpBlockRuleService
    {
        #region Field

        private readonly IRepository<IpBlockRule> _ipBlockRuleRepository;

        #endregion Field

        #region Ctor

        public IpBlockRuleService(IRepository<IpBlockRule> ipBlockRuleRepository)
        {
            _ipBlockRuleRepository = ipBlockRuleRepository;
        }

        #endregion ctor

        #region Methods

        public async Task<IpBlockRule> GetIpBlockRuleByIdAsync(int id)
        {
            return await _ipBlockRuleRepository.GetByIdAsync(id);
        }

        public async Task<IList<IPAddress>> GetAllBlockedIPAddressesAsync()
        {
            var query = from bi in _ipBlockRuleRepository.Table
                        where !bi.IsAllowed
                        select IPAddress.Parse(bi.IpAddress);

            return await query.ToListAsync();
        }

        public async Task<IPagedList<IpBlockRule>> GetIpBlockRulesAsync(DateTime? createdFromUtc = null,
            DateTime? createdToUtc = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = from cr in _ipBlockRuleRepository.Table
                        where (createdToUtc == null || cr.CreatedOnUtc.Date <= createdToUtc.Value.Date) &&
                        (createdFromUtc == null || cr.CreatedOnUtc.Date >= createdFromUtc.Value.Date)
                        orderby cr.CreatedOnUtc descending
                        select cr;

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        public async Task InsertIpBlockRuleAsync(IpBlockRule ipBlockRule)
        {
            await _ipBlockRuleRepository.InsertAsync(ipBlockRule);
        }

        public async Task UpdateIpBlockRuleAsync(IpBlockRule ipBlockRule)
        {
            await _ipBlockRuleRepository.UpdateAsync(ipBlockRule);
        }

        public async Task DeleteIpBlockRuleAsync(IpBlockRule ipBlockRule)
        {
            await _ipBlockRuleRepository.DeleteAsync(ipBlockRule);
        }

        #endregion
    }
}
