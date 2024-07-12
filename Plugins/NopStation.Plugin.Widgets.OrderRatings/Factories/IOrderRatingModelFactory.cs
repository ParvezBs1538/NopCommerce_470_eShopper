using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Orders;
using NopStation.Plugin.Widgets.OrderRatings.Models;

namespace NopStation.Plugin.Widgets.OrderRatings.Factories
{
    public interface IOrderRatingModelFactory
    {
        Task<OrderRatingModel> PrepareOrderRatingModelAsync(Order order);

        Task<RateableOrderListModel> PrepareRateableOrderListModelAsync(IEnumerable<Order> orders);
    }
}
