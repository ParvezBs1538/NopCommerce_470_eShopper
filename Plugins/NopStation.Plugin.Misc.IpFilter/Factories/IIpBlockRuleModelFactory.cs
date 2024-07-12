using System.Threading.Tasks;
using NopStation.Plugin.Misc.IpFilter.Domain;
using NopStation.Plugin.Misc.IpFilter.Models;

namespace NopStation.Plugin.Misc.IpFilter.Factories
{
    public interface IIpBlockRuleModelFactory
    {
        Task<IpBlockRuleSearchModel> PrepareIpBlockRuleSearchModelAsync(IpBlockRuleSearchModel searchModel);
        
        Task<IpBlockRuleListModel> PrepareIpBlockRuleListModelAsync(IpBlockRuleSearchModel searchModel);
        
        Task<IpBlockRuleModel> PrepareIpBlockRuleModelAsync(IpBlockRuleModel model, IpBlockRule ipBlockRule, bool excludeProperties = false);
    }
}