using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Data;
using NopStation.Plugin.Shipping.Redx.Domains;

namespace NopStation.Plugin.Shipping.Redx.Services
{
    public class RedxShipmentService : IRedxShipmentService
    {
        #region Fields

        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<RedxShipment> _redxShipmentRepository;

        #endregion

        #region ctor

        public RedxShipmentService(IRepository<Order> orderRepository,
            IRepository<RedxShipment> redxShipmentRepository)
        {
            _orderRepository = orderRepository;
            _redxShipmentRepository = redxShipmentRepository;
        }

        #endregion ctor

        #region Methods

        public async Task InsertShipmentAsync(RedxShipment redxShipment)
        {
            await _redxShipmentRepository.InsertAsync(redxShipment);
        }

        public async Task UpdateShipmentAsync(RedxShipment redxShipment)
        {
            await _redxShipmentRepository.UpdateAsync(redxShipment);
        }

        public async Task DeleteShipmentAsync(RedxShipment redxShipment)
        {
            await _redxShipmentRepository.DeleteAsync(redxShipment);
        }

        public async Task<IPagedList<RedxShipment>> GetAllRedxShipmentsAsync(string orderNumber = null, 
            string trackingId = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = from rs in _redxShipmentRepository.Table
                        join o in _orderRepository.Table on rs.OrderId equals o.Id
                        where (string.IsNullOrWhiteSpace(trackingId) || rs.TrackingId.Contains(trackingId)) &&
                            (string.IsNullOrWhiteSpace(orderNumber) || o.CustomOrderNumber.Contains(orderNumber))
                        orderby rs.OrderId descending
                        select rs;

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        public async Task<RedxShipment> GetRedxShipmentByIdAsync(int id)
        {
            if (id == 0)
                return null;

            return await _redxShipmentRepository.GetByIdAsync(id);
        }

        public async Task<RedxShipment> GetRedxShipmentByTrackingIdAsync(string trackingId)
        {
            var query = from rs in _redxShipmentRepository.Table
                        where rs.TrackingId == trackingId
                        select rs;

            return await query.FirstOrDefaultAsync();
        }

        public async Task<RedxShipment> GetRedxShipmentByShipmentIdAsync(int shipmentId)
        {
            var query = from rs in _redxShipmentRepository.Table
                        where rs.ShipmentId == shipmentId
                        select rs;

            return await query.FirstOrDefaultAsync();
        }

        #endregion Methods
    }
}