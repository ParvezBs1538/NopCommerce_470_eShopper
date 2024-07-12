using System.Threading.Tasks;
using NopStation.Plugin.Widgets.DMS.Areas.Admin.Models;
using NopStation.Plugin.Widgets.DMS.Domain;

namespace NopStation.Plugin.Widgets.DMS.Areas.Admin.Factories
{
    public interface IShipperModelFactory
    {
        ShipperSearchModel PrepareShipperSearchModel(ShipperSearchModel searchModel);

        Task<ShipperListModel> PrepareShipperListModelAsync(ShipperSearchModel searchModel);

        Task<ShipperModel> PrepareShipperModelAsync(ShipperModel model, Shipper shipper, bool excludeProperties = false);
    }
}