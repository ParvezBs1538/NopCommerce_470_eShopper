using System.Collections.Generic;
using System.Threading.Tasks;
using NopStation.Plugin.Widgets.AbandonedCarts.Models;

namespace NopStation.Plugin.Widgets.AbandonedCarts.Factories
{
    public interface IAbandonedCartFactory
    {
        Task<IList<ProductInfoModel>> PrepareProductInfoModelsByCustomerAsync(int customerId);
        Task<AbandonedCartsListModel> PrepareAbandonedCartsListModelAsync(AbandonedCartSearchModel searchModel);
        Task<CustomerAbandonmentListModel> PrepareCustomerAbandonmentListModelAsync(AbandonedCartSearchModel searchModel);
        AbandonedCartSearchModel PrepareAbandonedCartSearchModel(AbandonedCartSearchModel searchModel);
        Task<AbandonmentDetailsViewModel> PrepareAbandonedCartDetailViewModel(int id);
    }
}
