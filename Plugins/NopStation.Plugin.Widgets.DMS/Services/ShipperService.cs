using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Data;
using NopStation.Plugin.Widgets.DMS.Domain;

namespace NopStation.Plugin.Widgets.DMS.Services
{
    public class ShipperService : IShipperService
    {
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<Shipper> _shipperRepository;

        public ShipperService(IRepository<Customer> customerRepository,
            IRepository<Shipper> shipperRepository)
        {
            _customerRepository = customerRepository;
            _shipperRepository = shipperRepository;
        }

        public async Task InsertShipperAsync(Shipper shipper)
        {
            await _shipperRepository.InsertAsync(shipper);
        }

        public async Task UpdateShipperAsync(Shipper shipper)
        {
            await _shipperRepository.UpdateAsync(shipper);
        }

        public async Task DeleteShipperAsync(Shipper shipper)
        {
            await _shipperRepository.DeleteAsync(shipper);
        }

        public async Task<Shipper> GetShipperByCustomerIdAsync(int customerId)
        {
            if (customerId == 0)
                return null;

            return await _shipperRepository.Table.FirstOrDefaultAsync(x => x.CustomerId == customerId);
        }

        public async Task<Shipper> GetShipperByIdAsync(int id)
        {
            if (id == 0)
                return null;

            return await _shipperRepository.GetByIdAsync(id, cache => default);
        }

        public async Task<IPagedList<Shipper>> GetAllShippersAsync(string email = "", bool? active = null,
            int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = from cp in _shipperRepository.Table
                        join c in _customerRepository.Table on cp.CustomerId equals c.Id
                        where !c.Deleted
                        select new { Courier = cp, Customer = c };

            if (active.HasValue)
                query = from cp in query
                        where cp.Courier.Active == active.Value && cp.Customer.Active == active.Value
                        select cp;

            if (string.IsNullOrWhiteSpace(email))
                query = from cp in query
                        where cp.Customer.Email.Contains(email)
                        select cp;

            return await query.OrderBy(x => x.Customer.Email)
                .Select(x => x.Courier)
                .ToPagedListAsync(pageIndex, pageSize);
        }
    }
}
