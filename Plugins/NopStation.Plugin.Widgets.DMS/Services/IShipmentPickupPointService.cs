using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Widgets.DMS.Domain;

namespace NopStation.Plugin.Widgets.DMS.Services
{
    public interface IShipmentPickupPointService
    {
        Task DeleteShipmentPickupPointAsync(ShipmentPickupPoint shipmentPickupPoint);
        Task<IPagedList<ShipmentPickupPoint>> GetAllShipmentPickupPointsAsync(int pageIndex = 0, int pageSize = int.MaxValue);
        Task<ShipmentPickupPoint> GetShipmentPickupPointByIdAsync(int id);
        Task InsertShipmentPickupPointAsync(ShipmentPickupPoint shipmentPickupPoint);
        Task UpdateShipmentPickupPointAsync(ShipmentPickupPoint shipmentPickupPoint);
    }
}