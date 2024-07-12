using System.Threading.Tasks;
using NopStation.Plugin.Widgets.AffiliateStation.Models;

namespace NopStation.Plugin.Widgets.AffiliateStation.Factories
{
    public interface IAffiliateCustomerModelFactory
    {
        Task<AffiliateInfoModel> PrepareAffiliateInfoModelAsync();
    }
}