using System.Threading.Tasks;
using NopStation.Plugin.Widgets.AffiliateStation.Areas.Admin.Models;
using NopStation.Plugin.Widgets.AffiliateStation.Domains;

namespace NopStation.Plugin.Widgets.AffiliateStation.Areas.Admin.Factories
{
    public interface IOrderCommissionModelFactory
    {
        Task<OrderCommissionSearchModel> PrepareOrderCommissionSearchModelAsync(OrderCommissionSearchModel searchModel);
        Task<OrderCommissionListModel> PrepareOrderCommissionListModelAsync(OrderCommissionSearchModel searchModel);
        Task<OrderCommissionModel> PrepareOrderCommissionModelAsync(OrderCommissionModel model, OrderCommission orderCommission, bool excludeProperties = false);
        Task<OrderCommissionAggreratorModel> PrepareCommissionAggregatorModelAsync(OrderCommissionSearchModel searchModel);
    }
}
