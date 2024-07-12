using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Widgets.DMS.Domain;

namespace NopStation.Plugin.Widgets.DMS.Services
{
    public interface IShipperService
    {
        Task InsertShipperAsync(Shipper shipper);

        Task UpdateShipperAsync(Shipper shipper);

        Task DeleteShipperAsync(Shipper shipper);

        Task<Shipper> GetShipperByCustomerIdAsync(int customerId);

        Task<Shipper> GetShipperByIdAsync(int id);

        Task<IPagedList<Shipper>> GetAllShippersAsync(string email = "", bool? active = null,
            int pageIndex = 0, int pageSize = int.MaxValue);
    }
}
