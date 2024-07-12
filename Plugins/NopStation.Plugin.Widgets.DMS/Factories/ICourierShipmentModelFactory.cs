using System.Threading.Tasks;
using Nop.Core.Domain.Shipping;
using NopStation.Plugin.Widgets.DMS.Domain;
using NopStation.Plugin.Widgets.DMS.Models;

namespace NopStation.Plugin.Widgets.DMS.Factories
{
    public interface ICourierShipmentModelFactory
    {
        Task<CourierShipmentsModel> PrepareCourierShipmentsOverviewModelAsync(CourierShipmentsModel model, Shipper shipper, CourierShipmentsModel command, ShipmentSearchModel searchModel);
        Task<CourierShipmentDetailsModel> PrepareShipmentDetailsModelAsync(Shipment shipment, CourierShipment courierShipment);
    }
}
