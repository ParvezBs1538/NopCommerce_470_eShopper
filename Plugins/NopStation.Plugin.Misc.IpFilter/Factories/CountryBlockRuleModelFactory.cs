using System;
using System.Linq;
using System.Threading.Tasks;
using NopStation.Plugin.Misc.IpFilter.Domain;
using NopStation.Plugin.Misc.IpFilter.Models;
using NopStation.Plugin.Misc.IpFilter.Services;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Models.Extensions;

namespace NopStation.Plugin.Misc.IpFilter.Factories
{
    public class CountryBlockRuleModelFactory : ICountryBlockRuleModelFactory
    {
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ICountryBlockRuleService _countryBlockRuleService;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly ICountryService _countryService;

        public CountryBlockRuleModelFactory(IDateTimeHelper dateTimeHelper,
            ICountryBlockRuleService countryBlockRuleService,
            IBaseAdminModelFactory baseAdminModelFactory,
            ICountryService countryService)
        {
            _dateTimeHelper = dateTimeHelper;
            _countryBlockRuleService = countryBlockRuleService;
            _baseAdminModelFactory = baseAdminModelFactory;
            _countryService = countryService;
        }

        public Task<CountryBlockRuleSearchModel> PrepareCountryBlockRuleSearchModelAsync(CountryBlockRuleSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.SetGridPageSize();

            return Task.FromResult(searchModel);
        }

        public async Task<CountryBlockRuleListModel> PrepareCountryBlockRuleListModelAsync(CountryBlockRuleSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var createdFromUtc = searchModel.CreatedFrom.HasValue
               ? (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.CreatedFrom.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync())
               : null;
            var createdToUtc = searchModel.CreatedTo.HasValue
               ? (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.CreatedTo.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync())
               : null;

            var countryBlockRules = await _countryBlockRuleService.GetCountryBlockRulesAsync(
                createdFromUtc: createdFromUtc,
                createdToUtc: createdToUtc,
                pageIndex: searchModel.Page - 1, 
                pageSize: searchModel.PageSize);

            var model = await new CountryBlockRuleListModel().PrepareToGridAsync(searchModel, countryBlockRules, () =>
            {
                return countryBlockRules.SelectAwait(async countryBlockRule =>
                {
                    return await PrepareCountryBlockRuleModelAsync(null, countryBlockRule, true);
                });
            });

            return model;
        }

        public async Task<CountryBlockRuleModel> PrepareCountryBlockRuleModelAsync(CountryBlockRuleModel model, CountryBlockRule countryBlockRule, bool excludeProperties = false)
        {
            if (countryBlockRule != null)
            {
                if (model == null)
                {
                    model = countryBlockRule.ToModel<CountryBlockRuleModel>();
                    model.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(countryBlockRule.CreatedOnUtc);
                    model.CountryName = (await _countryService.GetCountryByIdAsync(countryBlockRule.CountryId))?.Name;
                }
            }

            if (!excludeProperties)
            {
                await _baseAdminModelFactory.PrepareCountriesAsync(model.AvailableCountries, false);
            }

            return model;
        }
    }
}
