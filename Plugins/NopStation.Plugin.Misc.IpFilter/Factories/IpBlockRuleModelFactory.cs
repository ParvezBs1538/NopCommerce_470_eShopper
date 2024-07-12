using System;
using System.Linq;
using System.Threading.Tasks;
using NopStation.Plugin.Misc.IpFilter.Domain;
using NopStation.Plugin.Misc.IpFilter.Models;
using NopStation.Plugin.Misc.IpFilter.Services;
using Nop.Services.Helpers;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Models.Extensions;

namespace NopStation.Plugin.Misc.IpFilter.Factories
{
    public class IpBlockRuleModelFactory : IIpBlockRuleModelFactory
    {
        private readonly IIpBlockRuleService _ipBlockRuleService;
        private readonly IDateTimeHelper _dateTimeHelper;

        public IpBlockRuleModelFactory(IIpBlockRuleService ipBlockRuleService,
            IDateTimeHelper dateTimeHelper)
        {
            _ipBlockRuleService = ipBlockRuleService;
            _dateTimeHelper = dateTimeHelper;
        }

        public Task<IpBlockRuleSearchModel> PrepareIpBlockRuleSearchModelAsync(IpBlockRuleSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.SetGridPageSize();

            return Task.FromResult(searchModel);
        }

        public async Task<IpBlockRuleListModel> PrepareIpBlockRuleListModelAsync(IpBlockRuleSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var createdFromUtc = searchModel.CreatedFrom.HasValue
               ? (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.CreatedFrom.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync())
               : null;
            var createdToUtc = searchModel.CreatedTo.HasValue
               ? (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.CreatedTo.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync())
               : null;

            var ipBlockRules = await _ipBlockRuleService.GetIpBlockRulesAsync(
                createdFromUtc: createdFromUtc,
                createdToUtc: createdToUtc,
                pageIndex: searchModel.Page - 1, 
                pageSize: searchModel.PageSize);

            var model = await new IpBlockRuleListModel().PrepareToGridAsync(searchModel, ipBlockRules, () =>
            {
                return ipBlockRules.SelectAwait(async ipBlockRule =>
                {
                    return await PrepareIpBlockRuleModelAsync(null, ipBlockRule, true);
                });
            });

            return model;
        }

        public async Task<IpBlockRuleModel> PrepareIpBlockRuleModelAsync(IpBlockRuleModel model, IpBlockRule ipBlockRule, bool excludeProperties = false)
        {
            if (ipBlockRule != null)
            {
                if (model == null)
                {
                    model = ipBlockRule.ToModel<IpBlockRuleModel>();
                    model.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(ipBlockRule.CreatedOnUtc);
                }
            }

            if (!excludeProperties)
            {

            }

            return model;
        }
    }
}
