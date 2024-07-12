using System.Threading.Tasks;
using NopStation.Plugin.Widgets.AffiliateStation.Areas.Admin.Models;
using NopStation.Plugin.Widgets.AffiliateStation.Domains;

namespace NopStation.Plugin.Widgets.AffiliateStation.Areas.Admin.Factories
{
    public interface IAffiliateCustomerModelFactory
    {
        Task<AffiliateCustomerSearchModel> PrepareAffiliateCustomerSearchModelAsync();
        Task<AffiliateCustomerListModel> PrepareAffiliateCustomerListModelAsync(AffiliateCustomerSearchModel searchModel);
        Task<AffiliateCustomerModel> PrepareAffiliateCustomerModelAsync(AffiliateCustomerModel model, AffiliateCustomer affiliateCustomer, bool excludeProperties = false);
    }
}
