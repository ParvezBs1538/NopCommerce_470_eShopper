using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Data;

namespace NopStation.Plugin.Misc.MergeGuestOrder.Services
{
    public class OrderServiceCustom : IOrderServiceCustom
    {
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<CustomerCustomerRoleMapping> _customerRoleMappingRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderNote> _orderNoteRepository;
        private readonly IRepository<Address> _addressRepository;

        public OrderServiceCustom(IRepository<Customer> customerRepository,
            IRepository<CustomerCustomerRoleMapping> customerRoleMappingRepository,
            IRepository<Order> orderRepository,
            IRepository<OrderNote> orderNoteRepository,
            IRepository<Address> addressRepository)
        {
            _customerRepository = customerRepository;
            _customerRoleMappingRepository = customerRoleMappingRepository;
            _orderRepository = orderRepository;
            _orderNoteRepository = orderNoteRepository;
            _addressRepository = addressRepository;
        }

        public async Task<IPagedList<Order>> SearchOrders(string email, CheckEmailInAddress checkIn, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = from o in _orderRepository.Table
                        join c in _customerRepository.Table on o.CustomerId equals c.Id
                        join crm in _customerRoleMappingRepository.Table on c.Id equals crm.CustomerId
                        where !o.Deleted && crm.CustomerRoleId == 4
                        select o;

            var query1 = from a in _addressRepository.Table
                         where a.Email == email
                         select a;

            query = checkIn switch
            {
                CheckEmailInAddress.Billing => from o in query
                                               where o.ShippingAddressId.HasValue && query1.Select(a => a.Id).Contains(o.ShippingAddressId.Value)
                                               select o,
                CheckEmailInAddress.Shipping => from o in query
                                                where query1.Select(a => a.Id).Contains(o.BillingAddressId)
                                                select o,
                _ => from o in query
                     where query1.Select(a => a.Id).Contains(o.BillingAddressId) ||
                     (o.ShippingAddressId.HasValue && query1.Select(a => a.Id).Contains(o.ShippingAddressId.Value))
                     select o,
            };

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        public async Task UpdateOrdersAsync(IList<Order> orders)
        {
            await _orderRepository.UpdateAsync(orders);
        }

        public async Task InsertOrderNotesAsync(IList<OrderNote> orderNotes)
        {
            await _orderNoteRepository.InsertAsync(orderNotes);
        }
    }
}
