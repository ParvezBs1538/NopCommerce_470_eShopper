using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Data;
using NopStation.Plugin.Widgets.DMS.Domain;

namespace NopStation.Plugin.Widgets.DMS.Services
{
    public class CourierShipmentService : ICourierShipmentService
    {
        private readonly IRepository<Shipment> _shipmentRepository;
        private readonly IRepository<CourierShipment> _courierShipmentRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<Address> _addressRepository;

        public CourierShipmentService(IRepository<Shipment> shipmentRepository,
            IRepository<CourierShipment> courierShipmentRepository,
            IRepository<Order> orderRepository,
            IRepository<Customer> customerRepository,
            IRepository<Address> addressRepository)
        {
            _shipmentRepository = shipmentRepository;
            _courierShipmentRepository = courierShipmentRepository;
            _orderRepository = orderRepository;
            _customerRepository = customerRepository;
            _addressRepository = addressRepository;
        }

        public async Task InsertCourierShipmentAsync(CourierShipment courierShipment)
        {
            await _courierShipmentRepository.InsertAsync(courierShipment);
        }

        public async Task UpdateCourierShipmentAsync(CourierShipment courierShipment)
        {
            await _courierShipmentRepository.UpdateAsync(courierShipment);
        }

        public async Task DeleteCourierShipmentAsync(CourierShipment courierShipment)
        {
            await _courierShipmentRepository.DeleteAsync(courierShipment);
        }

        public async Task<CourierShipment> GetCourierShipmentByShipmentIdAsync(int shipmentId)
        {
            if (shipmentId == 0)
                return null;

            return await _courierShipmentRepository.Table.FirstOrDefaultAsync(x => x.ShipmentId == shipmentId);
        }

        public async Task<CourierShipment> GetCourierShipmentByIdAsync(int id)
        {
            if (id == 0)
                return null;

            return await _courierShipmentRepository.GetByIdAsync(id, cache => default);
        }

        public async Task<IPagedList<CourierShipment>> GetAllCourierShipmentsAsync(
            int? orderId = null,
            string trackingNumber = null,
            string customOrderNumber = null,
            string email = null,
            int statusId = 0,
            int? shipperId = null,
            int? shipmentId = null,
            int? courierShipmentStatusId = null,
            DateTime? createdOnUtc = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue
            )
        {
            if (pageSize == int.MaxValue)
                --pageSize;

            var query = from cs in _courierShipmentRepository.Table
                        select cs;

            if (courierShipmentStatusId.HasValue)
                query = from cs in query
                        where cs.ShipmentStatusId == courierShipmentStatusId.Value
                        select cs;

            if (shipperId.HasValue && shipperId.Value > 0)
                query = from cs in query
                        where cs.ShipperId == shipperId.Value
                        select cs;

            if (createdOnUtc.HasValue)
                query = from cs in query
                        where cs.CreatedOnUtc >= createdOnUtc.Value
                        select cs;

            if (shipmentId.HasValue && shipmentId.Value > 0)
                query = query.Where(q => q.ShipmentId == shipmentId.Value);



            if (!string.IsNullOrEmpty(email) || !string.IsNullOrEmpty(customOrderNumber) || orderId.HasValue)
                query = from cs in query
                        join s in _shipmentRepository.Table on cs.ShipmentId equals s.Id
                        join o in _orderRepository.Table on s.OrderId equals o.Id
                        join ad in _addressRepository.Table on o.ShippingAddressId equals ad.Id
                        where (string.IsNullOrEmpty(email) || ad.Email.Contains(email)) &&
                        (string.IsNullOrEmpty(customOrderNumber) || o.CustomOrderNumber.Contains(customOrderNumber)) &&
                        (!orderId.HasValue || orderId < 1 || o.Id == orderId.Value)
                        select cs;

            if (!string.IsNullOrEmpty(trackingNumber))
                query = from cs in query
                        join s in _shipmentRepository.Table on cs.ShipmentId equals s.Id
                        where s.TrackingNumber != null && s.TrackingNumber == trackingNumber
                        select cs;

            if (statusId > 0)
            {
                query = from cs in query
                        join s in _shipmentRepository.Table on cs.ShipmentId equals s.Id
                        join o in _orderRepository.Table on s.OrderId equals o.Id
                        where o.ShippingStatusId == statusId
                        select cs;
            }

            return await query.OrderByDescending(x => x.CreatedOnUtc).ToPagedListAsync(pageIndex, pageSize);
        }
    }
}
