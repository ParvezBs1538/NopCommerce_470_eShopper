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
    public class IpRangeBlockRuleModelFactory : IIpRangeBlockRuleModelFactory
    {
        private readonly IIpRangeBlockRuleService _ipRangeBlockRuleService;
        private readonly IDateTimeHelper _dateTimeHelper;

        public IpRangeBlockRuleModelFactory(IIpRangeBlockRuleService ipRangeBlockRuleService,
            IDateTimeHelper dateTimeHelper)
        {
            _ipRangeBlockRuleService = ipRangeBlockRuleService;
            _dateTimeHelper = dateTimeHelper;
        }

        public Task<IpRangeBlockRuleSearchModel> PrepareIpRangeBlockRuleSearchModelAsync(IpRangeBlockRuleSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.SetGridPageSize();

            return Task.FromResult(searchModel);
        }

        public async Task<IpRangeBlockRuleListModel> PrepareIpRangeBlockRuleListModelAsync(IpRangeBlockRuleSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var createdFromUtc = searchModel.CreatedFrom.HasValue
               ? (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.CreatedFrom.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync())
               : null;
            var createdToUtc = searchModel.CreatedTo.HasValue
               ? (DateTime?)_dateTimeHelper.ConvertToUtcTime(searchModel.CreatedTo.Value, await _dateTimeHelper.GetCurrentTimeZoneAsync())
               : null;

            var ipRangeBlockRules = await _ipRangeBlockRuleService.GetIpRangeBlockRulesAsync(
                createdFromUtc: createdFromUtc,
                createdToUtc: createdToUtc,
                pageIndex: searchModel.Page - 1, 
                pageSize: searchModel.PageSize);

            var model = await new IpRangeBlockRuleListModel().PrepareToGridAsync(searchModel, ipRangeBlockRules, () =>
            {
                return ipRangeBlockRules.SelectAwait(async ipRangeBlockRule =>
                {
                    return await PrepareIpRangeBlockRuleModelAsync(null, ipRangeBlockRule, true);
                });
            });

            return model;
        }

        public async Task<IpRangeBlockRuleModel> PrepareIpRangeBlockRuleModelAsync(IpRangeBlockRuleModel model, IpRangeBlockRule ipRangeBlockRule, bool excludeProperties = false)
        {
            if (ipRangeBlockRule != null)
            {
                if (model == null)
                {
                    model = ipRangeBlockRule.ToModel<IpRangeBlockRuleModel>();
                    model.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(ipRangeBlockRule.CreatedOnUtc);
                }
            }

            if (!excludeProperties)
            {

            }

            return model;
        }
    }
}
