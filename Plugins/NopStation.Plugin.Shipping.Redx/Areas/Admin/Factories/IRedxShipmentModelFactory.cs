using System.Threading.Tasks;
using Nop.Core.Domain.Shipping;
using NopStation.Plugin.Shipping.Redx.Areas.Admin.Models;
using NopStation.Plugin.Shipping.Redx.Domains;

namespace NopStation.Plugin.Shipping.Redx.Areas.Admin.Factories
{
    public interface IRedxShipmentModelFactory
    {
        Task<RedxShipmentSearchModel> PrepareRedxShipmentSearchModelAsync(RedxShipmentSearchModel searchModel);

        Task<RedxShipmentListModel> PrepareRedxShipmentListModelAsync(RedxShipmentSearchModel searchModel);

        Task<RedxShipmentModel> PrepareRedxShipmentModelAsync(RedxShipmentModel model, RedxShipment redxShipment,
            Shipment shipment, bool excludeProperties = false);
    }
}