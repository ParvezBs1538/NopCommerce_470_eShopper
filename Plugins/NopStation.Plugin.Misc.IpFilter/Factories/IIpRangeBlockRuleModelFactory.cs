using System.Threading.Tasks;
using NopStation.Plugin.Misc.IpFilter.Domain;
using NopStation.Plugin.Misc.IpFilter.Models;

namespace NopStation.Plugin.Misc.IpFilter.Factories
{
    public interface IIpRangeBlockRuleModelFactory
    {
        Task<IpRangeBlockRuleSearchModel> PrepareIpRangeBlockRuleSearchModelAsync(IpRangeBlockRuleSearchModel searchModel);
        
        Task<IpRangeBlockRuleListModel> PrepareIpRangeBlockRuleListModelAsync(IpRangeBlockRuleSearchModel searchModel);
        
        Task<IpRangeBlockRuleModel> PrepareIpRangeBlockRuleModelAsync(IpRangeBlockRuleModel model, IpRangeBlockRule ipRangeBlockRule, bool excludeProperties = false);
    }
}