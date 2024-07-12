using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using NopStation.Plugin.Widgets.DMS.Domain;

namespace NopStation.Plugin.Widgets.DMS.Services
{
    public interface IShipperDeviceService
    {
        Task DeleteShipperDeviceAsync(ShipperDevice device);

        Task InsertShipperDeviceAsync(ShipperDevice device);

        Task UpdateShipperDeviceAsync(ShipperDevice device);

        Task<ShipperDevice> GetShipperDeviceByIdAsync(int deviceId);

        Task<ShipperDevice> GetShipperDeviceByDeviceIdAsync(string deviceToken);

        Task<ShipperDevice> GetShipperDeviceByCustomerAsync(Customer customer);

        Task<ShipperDevice> GetShipperDeviceByCurrentCustomerAsync();

        Task<ShipperDevice> GetShipperDeviceByCustomerIdAsync(int customerId);

        Task<IPagedList<ShipperDevice>> SearchShipperDevicesAsync(
            int customerId = 0,
            IList<int> dtids = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue
            );

        IList<ShipperDevice> GetShipperDeviceByIds(int[] deviceByIds);

        Task DeleteShipperDevicesAsync(IList<ShipperDevice> devices);
        Task<IPagedList<ShipperDevice>> GetAllShipperDeviceAsync(IList<int> customerIds = null, int pageIndex = 0, int pageSize = int.MaxValue);
    }
}
