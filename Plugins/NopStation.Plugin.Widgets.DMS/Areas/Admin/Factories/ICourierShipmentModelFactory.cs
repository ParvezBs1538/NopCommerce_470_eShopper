using System.Threading.Tasks;
using Nop.Core.Domain.Shipping;
using NopStation.Plugin.Widgets.DMS.Areas.Admin.Models;
using NopStation.Plugin.Widgets.DMS.Domain;

namespace NopStation.Plugin.Widgets.DMS.Areas.Admin.Factories
{
    public interface ICourierShipmentModelFactory
    {
        Task<CourierShipmentModel> PrepareCourierShipmentModelAsync(CourierShipmentModel model,
            CourierShipment courierShipment, Shipment shipment, bool excludeProperties = false);

        Task<CourierShipmentSearchModel> PrepareCourierShipmentSearchModelAsync(CourierShipmentSearchModel searchModel);

        Task<CourierShipmentListModel> PrepareCourierShipmentListModelAsync(CourierShipmentSearchModel searchModel);
    }
}