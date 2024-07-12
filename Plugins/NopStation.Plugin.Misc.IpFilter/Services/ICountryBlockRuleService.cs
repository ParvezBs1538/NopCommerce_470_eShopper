using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Misc.IpFilter.Domain;

namespace NopStation.Plugin.Misc.IpFilter.Services
{
    public interface ICountryBlockRuleService
    {
        Task<CountryBlockRule> GetCountryBlockRuleByIdAsync(int id);

        Task<IList<string>> GetAllBlockedCountryCodesAsync();

        Task<IPagedList<CountryBlockRule>> GetCountryBlockRulesAsync(DateTime? createdFromUtc = null,
            DateTime? createdToUtc = null, int pageIndex = 0, int pageSize = int.MaxValue);

        Task InsertCountryBlockRuleAsync(CountryBlockRule blockCountryRule);

        Task UpdateCountryBlockRuleAsync(CountryBlockRule blockCountryRule);

        Task DeleteCountryBlockRuleAsync(CountryBlockRule blockCountryRule);
    }
}
