using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Shipping.Redx.Domains;

namespace NopStation.Plugin.Shipping.Redx.Services
{
    public interface IRedxShipmentService
    {
        Task InsertShipmentAsync(RedxShipment redxShipment);

        Task UpdateShipmentAsync(RedxShipment redxShipment);

        Task DeleteShipmentAsync(RedxShipment redxShipment);

        Task<RedxShipment> GetRedxShipmentByIdAsync(int id);

        Task<RedxShipment> GetRedxShipmentByTrackingIdAsync(string trackingId);

        Task<RedxShipment> GetRedxShipmentByShipmentIdAsync(int shipmentId);

        Task<IPagedList<RedxShipment>> GetAllRedxShipmentsAsync(string orderNumber = null,
            string trackingId = null, int pageIndex = 0, int pageSize = int.MaxValue);
    }
}