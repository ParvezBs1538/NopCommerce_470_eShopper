using System.Threading.Tasks;
using NopStation.Plugin.Widgets.DMS.Areas.Admin.Models.ShipmentPickupPoint;
using NopStation.Plugin.Widgets.DMS.Domain;

namespace NopStation.Plugin.Widgets.DMS.Areas.Admin.Factories
{
    public interface IShipmentPickupPointModelFactory
    {

        ShipmentPickupPointSearchModel PrepareShipmentPickupPointSearchModel(ShipmentPickupPointSearchModel searchModel);

        Task<ShipmentPickupPointListModel> PrepareShipmentPickupPointListModelAsync(ShipmentPickupPointSearchModel searchModel);

        Task<ShipmentPickupPointModel> PrepareShipmentPickupPointModelAsync(ShipmentPickupPointModel model, ShipmentPickupPoint shipmentPickupPoint, bool excludeProperties = false);
    }
}
