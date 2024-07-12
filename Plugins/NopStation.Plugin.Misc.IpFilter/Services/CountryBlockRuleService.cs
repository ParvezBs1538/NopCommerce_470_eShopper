using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Data;
using NopStation.Plugin.Misc.IpFilter.Domain;

namespace NopStation.Plugin.Misc.IpFilter.Services
{
    public class CountryBlockRuleService : ICountryBlockRuleService
    {
        #region Field

        private readonly IRepository<Country> _countryRepository;
        private readonly IRepository<CountryBlockRule> _countryBlockRuleRepository;

        #endregion Field

        #region Ctor

        public CountryBlockRuleService(IRepository<Country> countryRepository,
            IRepository<CountryBlockRule> countryBlockRuleRepository)
        {
            _countryRepository = countryRepository;
            _countryBlockRuleRepository = countryBlockRuleRepository;
        }

        #endregion ctor

        #region Methods

        public async Task<CountryBlockRule> GetCountryBlockRuleByIdAsync(int id)
        {
            return await _countryBlockRuleRepository.GetByIdAsync(id);
        }

        public async Task<IList<string>> GetAllBlockedCountryCodesAsync()
        {
            var query = from cr in _countryBlockRuleRepository.Table
                        join c in _countryRepository.Table on cr.CountryId equals c.Id
                        select c.TwoLetterIsoCode;

            return await query.ToListAsync();
        }

        public async Task InsertCountryBlockRuleAsync(CountryBlockRule countryBlockRule)
        {
            await _countryBlockRuleRepository.InsertAsync(countryBlockRule);
        }

        public async Task UpdateCountryBlockRuleAsync(CountryBlockRule countryBlockRule)
        {
            await _countryBlockRuleRepository.UpdateAsync(countryBlockRule);
        }

        public async Task DeleteCountryBlockRuleAsync(CountryBlockRule countryBlockRule)
        {
            await _countryBlockRuleRepository.DeleteAsync(countryBlockRule);
        }

        public async Task<IPagedList<CountryBlockRule>> GetCountryBlockRulesAsync(DateTime? createdFromUtc = null,
            DateTime? createdToUtc = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = from cr in _countryBlockRuleRepository.Table
                        where (createdToUtc == null || cr.CreatedOnUtc.Date <= createdToUtc.Value.Date) &&
                        (createdFromUtc == null || cr.CreatedOnUtc.Date >= createdFromUtc.Value.Date)
                        orderby cr.CreatedOnUtc descending
                        select cr;

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        #endregion
    }
}
