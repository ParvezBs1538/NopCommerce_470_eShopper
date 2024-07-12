using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Data;
using NopStation.Plugin.Widgets.OrderRatings.Domain;

namespace NopStation.Plugin.Widgets.OrderRatings.Services
{
    public class OrderRatingService : IOrderRatingService
    {
        #region Fields

        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderRating> _orderRatingRepository;

        #endregion

        #region Ctor

        public OrderRatingService(IRepository<Order> orderRepository,
            IRepository<OrderRating> orderRatingRepository)
        {
            _orderRepository = orderRepository;
            _orderRatingRepository = orderRatingRepository;
        }

        #endregion

        #region methods

        public async Task DeleteOrderRatingAsync(OrderRating orderRating)
        {
            await _orderRatingRepository.DeleteAsync(orderRating);
        }

        public async Task InsertOrderRatingAsync(OrderRating orderRating)
        {
            await _orderRatingRepository.InsertAsync(orderRating);
        }

        public async Task UpdateOrderRatingAsync(OrderRating orderRating)
        {
            await _orderRatingRepository.UpdateAsync(orderRating);
        }

        public async Task<OrderRating> GetOrderRatingByOrderIdAsync(int orderId)
        {
            if (orderId == 0)
                return null;

            var query = from o in _orderRepository.Table
                        join or in _orderRatingRepository.Table on o.Id equals or.OrderId
                        where !o.Deleted && o.Id == orderId
                        select or;

            return await query.FirstOrDefaultAsync();
        }

        public async Task<IPagedList<Order>> GetAllOrdersAsync(int customerId = 0, bool ignoreSkipped = false,
            bool ignoreRated = false, bool? rateable = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = from o in _orderRepository.Table
                        where !o.Deleted &&
                        (customerId == 0 || o.CustomerId == customerId) &&
                        (rateable == null || o.OrderStatusId == (int)OrderStatus.Complete == rateable.Value)
                        select o;

            if (ignoreSkipped)
            {
                var query1 = from or in _orderRatingRepository.Table
                             join o in query on or.OrderId equals o.Id
                             where or.RatedOnUtc == null
                             select or.OrderId;

                query = query.Where(o => !query1.Contains(o.Id));
            }

            if (ignoreRated)
            {
                var query1 = from or in _orderRatingRepository.Table
                             join o in query on or.OrderId equals o.Id
                             where or.RatedOnUtc.HasValue
                             select or.OrderId;

                query = query.Where(o => !query1.Contains(o.Id));
            }

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        #endregion
    }
}
