using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firewall;
using Nop.Core;
using Nop.Data;
using NopStation.Plugin.Misc.IpFilter.Domain;
using NopStation.Plugin.Misc.IpFilter.Helper.Extensions;

namespace NopStation.Plugin.Misc.IpFilter.Services
{
    public class IpRangeBlockRuleService : IIpRangeBlockRuleService
    {
        #region Field

        private readonly IRepository<IpRangeBlockRule> _ipRangeBlockRuleRepository;

        #endregion Field

        #region Ctor

        public IpRangeBlockRuleService(IRepository<IpRangeBlockRule> ipRangeBlockRuleRepository)
        {
            _ipRangeBlockRuleRepository = ipRangeBlockRuleRepository;
        }

        #endregion ctor

        #region Methods

        public async Task<IpRangeBlockRule> GetIpRangeBlockRuleByIdAsync(int id)
        {
            return await _ipRangeBlockRuleRepository.GetByIdAsync(id);
        }

        public async Task<IList<CIDRNotation>> GetAllCIDRNotationsAsync()
        {
            var query = from irr in _ipRangeBlockRuleRepository.Table
                        select irr;

            var cidrNotations = new List<CIDRNotation>();

            foreach (var rangeRule in await query.ToListAsync())
            {
                cidrNotations.AddRange(new IPRangeToCIDRNotationConvert()
                    .IPRange2CIDRNotation(rangeRule.FromIpAddress, rangeRule.ToIpAddress)
                    .Select(cidrNotation => CIDRNotation.Parse(cidrNotation)));
            }

            return cidrNotations;
        }

        public async Task<IPagedList<IpRangeBlockRule>> GetIpRangeBlockRulesAsync(DateTime? createdFromUtc = null,
            DateTime? createdToUtc = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = from irr in _ipRangeBlockRuleRepository.Table
                        where (createdToUtc == null || irr.CreatedOnUtc.Date <= createdToUtc.Value.Date) &&
                        (createdFromUtc == null || irr.CreatedOnUtc.Date >= createdFromUtc.Value.Date)
                        orderby irr.CreatedOnUtc descending
                        select irr;

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        public async Task InsertIpRangeBlockRuleAsync(IpRangeBlockRule ipRangeBlockRule)
        {
            await _ipRangeBlockRuleRepository.InsertAsync(ipRangeBlockRule);
        }

        public async Task UpdateIpRangeBlockRuleAsync(IpRangeBlockRule ipRangeBlockRule)
        {
            await _ipRangeBlockRuleRepository.UpdateAsync(ipRangeBlockRule);
        }

        public async Task DeleteIpRangeBlockRuleAsync(IpRangeBlockRule ipRangeBlockRule)
        {
            await _ipRangeBlockRuleRepository.DeleteAsync(ipRangeBlockRule);
        }

        #endregion
    }
}
