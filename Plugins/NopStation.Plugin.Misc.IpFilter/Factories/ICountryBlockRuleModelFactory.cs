using System.Threading.Tasks;
using NopStation.Plugin.Misc.IpFilter.Domain;
using NopStation.Plugin.Misc.IpFilter.Models;

namespace NopStation.Plugin.Misc.IpFilter.Factories
{
    public interface ICountryBlockRuleModelFactory
    {
        Task<CountryBlockRuleSearchModel> PrepareCountryBlockRuleSearchModelAsync(CountryBlockRuleSearchModel searchModel);
        
        Task<CountryBlockRuleListModel> PrepareCountryBlockRuleListModelAsync(CountryBlockRuleSearchModel searchModel);
        
        Task<CountryBlockRuleModel> PrepareCountryBlockRuleModelAsync(CountryBlockRuleModel model, CountryBlockRule countryBlockRule, bool excludeProperties = false);
    }
}