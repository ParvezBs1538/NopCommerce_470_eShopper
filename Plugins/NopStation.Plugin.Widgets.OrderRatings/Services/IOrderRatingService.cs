using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Orders;
using NopStation.Plugin.Widgets.OrderRatings.Domain;

namespace NopStation.Plugin.Widgets.OrderRatings.Services
{
    public interface IOrderRatingService
    {
        Task DeleteOrderRatingAsync(OrderRating orderRating);

        Task InsertOrderRatingAsync(OrderRating orderRating);

        Task UpdateOrderRatingAsync(OrderRating orderRating);

        Task<OrderRating> GetOrderRatingByOrderIdAsync(int orderId);

        Task<IPagedList<Order>> GetAllOrdersAsync(int customerId = 0, bool ignoreSkipped = false, 
            bool ignoreRated = false, bool? rateable = null, int pageIndex = 0, int pageSize = int.MaxValue);
    }
}
