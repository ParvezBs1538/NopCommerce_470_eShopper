using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Data;
using NopStation.Plugin.Widgets.DMS.Domain;

namespace NopStation.Plugin.Widgets.DMS.Services
{
    public class ShipperDeviceService : IShipperDeviceService
    {

        #region Fields

        private readonly IRepository<ShipperDevice> _deviceRepository;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public ShipperDeviceService(
            IRepository<ShipperDevice> deviceRepository,
            IWorkContext workContext
            )
        {
            _deviceRepository = deviceRepository;
            _workContext = workContext;
        }

        #endregion

        #region Methods

        public async Task DeleteShipperDeviceAsync(ShipperDevice device)
        {
            await _deviceRepository.DeleteAsync(device);
        }

        public async Task InsertShipperDeviceAsync(ShipperDevice device)
        {
            await _deviceRepository.InsertAsync(device);
        }

        public async Task UpdateShipperDeviceAsync(ShipperDevice device)
        {
            await _deviceRepository.UpdateAsync(device);
        }

        public async Task<ShipperDevice> GetShipperDeviceByIdAsync(int deviceId)
        {
            if (deviceId == 0)
                return null;

            return await _deviceRepository.GetByIdAsync(deviceId, cache => default);
        }

        public async Task<ShipperDevice> GetShipperDeviceByDeviceIdAsync(string deviceToken)
        {
            if (string.IsNullOrEmpty(deviceToken))
                throw new ArgumentNullException(nameof(deviceToken));

            return await _deviceRepository.Table.FirstOrDefaultAsync(x => x.DeviceToken == deviceToken);
        }

        public async Task<IPagedList<ShipperDevice>> GetAllShipperDeviceAsync(IList<int> customerIds = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            if (pageSize == int.MaxValue)
                pageSize--;

            var query = _deviceRepository.Table;

            if (customerIds != null)
                query = query.Where(x => customerIds.Contains(x.CustomerId));

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }
        public async Task<ShipperDevice> GetShipperDeviceByCustomerAsync(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            return await _deviceRepository.Table.FirstOrDefaultAsync(x => x.CustomerId == customer.Id);
        }


        public async Task<ShipperDevice> GetShipperDeviceByCurrentCustomerAsync()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();

            return await _deviceRepository.Table.FirstOrDefaultAsync(x => x.CustomerId == customer.Id);
        }

        public async Task<ShipperDevice> GetShipperDeviceByCustomerIdAsync(int customerId)
        {
            if (customerId < 1)
                throw new ArgumentNullException(nameof(customerId));

            return await _deviceRepository.Table.FirstOrDefaultAsync(x => x.CustomerId == customerId);
        }

        public async Task<IPagedList<ShipperDevice>> SearchShipperDevicesAsync(
            int customerId = 0,
            IList<int> dtids = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue
            //int storeId = 0
            )
        {
            var query = _deviceRepository.Table;
            if (dtids != null && dtids.Any())
                query = query.Where(x => dtids.Contains(x.DeviceTypeId));

            if (customerId > 0)
                query = query.Where(x => x.CustomerId == customerId);

            //if (storeId > 0)
            //    query = query.Where(x => x.StoreId == storeId);

            query = query.OrderByDescending(e => e.Id);

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        public IList<ShipperDevice> GetShipperDeviceByIds(int[] deviceIds)
        {
            if (deviceIds == null || deviceIds.Length == 0)
                return new List<ShipperDevice>();

            var devices = _deviceRepository.Table.Where(x => deviceIds.Contains(x.Id)).ToList();

            var sortedDevices = new List<ShipperDevice>();
            foreach (var id in deviceIds)
            {
                var device = devices.Find(x => x.Id == id);
                if (device != null)
                    sortedDevices.Add(device);
            }
            return sortedDevices;
        }

        public async Task DeleteShipperDevicesAsync(IList<ShipperDevice> devices)
        {
            await _deviceRepository.DeleteAsync(devices);
        }

        #endregion
    }
}
