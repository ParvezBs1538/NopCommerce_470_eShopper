using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Widgets.DeliveryScheduler.Domains;

namespace NopStation.Plugin.Widgets.DeliveryScheduler.Services
{
    public interface IDeliverySlotService
    {
        Task DeleteDeliverySlotAsync(DeliverySlot deliverySlot);

        Task InsertDeliverySlotAsync(DeliverySlot deliverySlot);

        Task UpdateDeliverySlotAsync(DeliverySlot deliverySlot);

        Task<DeliverySlot> GetDeliverySlotByIdAsync(int deliverySlotId);

        bool IsActiveDeliverySlotExits();

        Task<IPagedList<DeliverySlot>> SearchDeliverySlotsAsync(int shippingMethodId = 0, bool? active = null,
            int storeId = 0, int pageIndex = 0, int pageSize = int.MaxValue);

        IList<int> GetShippingMethodIdsWithAccess(DeliverySlot deliverySlot);

        Task<IList<DeliverySlotShippingMethodMapping>> GetDeliverySlotShippingMethodMappingsAsync(DeliverySlot deliverySlot);

        Task DeleteDeliverySlotShippingMethodMappingAsync(DeliverySlotShippingMethodMapping mapping);

        Task InsertDeliverySlotShippingMethodMappingAsync(DeliverySlotShippingMethodMapping mapping);

        Task InsertDeliverySlotShippingMethodMappingAsync(DeliverySlot deliverySlot, int shippingMethodId);
    }
}