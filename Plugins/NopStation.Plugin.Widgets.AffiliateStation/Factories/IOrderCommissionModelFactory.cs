using System.Threading.Tasks;
using NopStation.Plugin.Widgets.AffiliateStation.Domains;
using NopStation.Plugin.Widgets.AffiliateStation.Models;

namespace NopStation.Plugin.Widgets.AffiliateStation.Factories
{
    public interface IOrderCommissionModelFactory
    {
        Task<AffiliatedOrderModel> PrepareAffiliatedOrderSummaryModelAsync(AffiliateCustomer affiliateCustomer, AffiliatedOrderPagingFilteringModel command);
    }
}