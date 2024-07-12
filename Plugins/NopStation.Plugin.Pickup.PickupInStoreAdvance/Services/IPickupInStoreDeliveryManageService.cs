using System.Collections.Generic;
using System.Threading.Tasks;
using NopStation.Plugin.Pickup.PickupInStoreAdvance.Areas.Admin.Models;
using NopStation.Plugin.Pickup.PickupInStoreAdvance.Domain;

namespace NopStation.Plugin.Pickup.PickupInStoreAdvance.Services
{
    public interface IPickupInStoreDeliveryManageService
    {
        Task InsertAsync(PickupInStoreDeliveryManage pickupInStoreDeliveryManage);
        Task UpdateAsync(PickupInStoreDeliveryManage pickupInStoreDeliveryManage);
        Task DeleteAsync(PickupInStoreDeliveryManage pickupInStoreDeliveryManage);
        Task<PickupInStoreDeliveryManage> GetPickupInStoreDeliverManageByOrderIdAsync(int orderId);
        Task<List<PickupInStoreDeliveryManage>> GetAllOrdersAsync(PickupInStoreDeliveryManageSearchModel searchModel);
        Task<bool> MarkAsReadyToPickupAsync(int orderId);
        Task<bool> MarkAsPickedByCustomerAsync(int orderId);
    }
}