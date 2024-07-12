using System.Threading.Tasks;
using NopStation.Plugin.Widgets.DeliveryScheduler.Areas.Admin.Models;
using NopStation.Plugin.Widgets.DeliveryScheduler.Domains;

namespace NopStation.Plugin.Widgets.DeliveryScheduler.Areas.Admin.Factories
{
    public interface IDeliverySlotModelFactory
    {
        Task<DeliverySlotSearchModel> PrepareDeliverySlotSearchModelAsync(DeliverySlotSearchModel deliverySlotSearchModel);

        Task<DeliverySlotListModel> PrepareDeliverySlotListModelAsync(DeliverySlotSearchModel searchModel);

        Task<DeliverySlotModel> PrepareDeliverySlotModelAsync(DeliverySlotModel deliverySlotModel, DeliverySlot deliverySlot, bool excludeProperties = false);
    }
}