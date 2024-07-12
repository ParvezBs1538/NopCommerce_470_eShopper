using System;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Widgets.DMS.Domain;

namespace NopStation.Plugin.Widgets.DMS.Services
{
    public interface ICourierShipmentService
    {
        Task InsertCourierShipmentAsync(CourierShipment courierShipment);

        Task UpdateCourierShipmentAsync(CourierShipment courierShipment);

        Task DeleteCourierShipmentAsync(CourierShipment courierShipment);

        Task<CourierShipment> GetCourierShipmentByShipmentIdAsync(int shipmentId);

        Task<CourierShipment> GetCourierShipmentByIdAsync(int id);

        Task<IPagedList<CourierShipment>> GetAllCourierShipmentsAsync(
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
            );
    }
}
