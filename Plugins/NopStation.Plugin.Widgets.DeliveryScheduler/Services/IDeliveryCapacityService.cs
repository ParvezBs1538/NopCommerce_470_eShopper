using System;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Widgets.DeliveryScheduler.Domains;

namespace NopStation.Plugin.Widgets.DeliveryScheduler.Services
{
    public interface IDeliveryCapacityService
    {
        Task DeleteDeliveryCapacityAsync(DeliveryCapacity deliveryCapacity);

        Task InsertDeliveryCapacityAsync(DeliveryCapacity deliveryCapacity);

        Task UpdateDeliveryCapacityAsync(DeliveryCapacity deliveryCapacity);

        Task<DeliveryCapacity> GetDeliveryCapacityAsync(int deliverySlotId, int shippingMethodId);

        Task<int> GetDeliveryCapacityForDateSlotAsync(DateTime date, DeliverySlot deliverySlot, int shippingMethodId);

        Task<IPagedList<DeliveryCapacity>> SearchDeliveryCapacitiesAsync(int shippingMethodId = 0, int deliverySlotId = 0,
            int pageIndex = 0, int pageSize = int.MaxValue);
    }
}