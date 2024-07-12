using System;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Widgets.DeliveryScheduler.Domains;

namespace NopStation.Plugin.Widgets.DeliveryScheduler.Services
{
    public interface ISpecialDeliveryCapacityService
    {
        Task DeleteSpecialDeliveryCapacityAsync(SpecialDeliveryCapacity specialDeliveryCapacity);

        Task InsertSpecialDeliveryCapacityAsync(SpecialDeliveryCapacity specialDeliveryCapacity);

        Task UpdateSpecialDeliveryCapacityAsync(SpecialDeliveryCapacity specialDeliveryCapacity);

        Task<SpecialDeliveryCapacity> GetSpecialDeliveryCapacityByIdAsync(int specialDeliveryCapacityId);

        Task<SpecialDeliveryCapacity> GetSpecialDeliveryCapacityAsync(DateTime specialDate, int deliverySlotId, int shippingMethodId);

        Task<IPagedList<SpecialDeliveryCapacity>> SearchSpecialDeliveryCapacitiesAsync(DateTime? specialFromDate = null,
            DateTime? specialToDate = null, int deliverySlotId = 0, int shippingMethodId = 0, int storeId = 0, int pageIndex = 0, int pageSize = int.MaxValue);
    }
}