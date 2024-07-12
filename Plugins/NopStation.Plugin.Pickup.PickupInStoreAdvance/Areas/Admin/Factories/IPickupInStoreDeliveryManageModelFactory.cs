using System.Threading.Tasks;
using NopStation.Plugin.Pickup.PickupInStoreAdvance.Areas.Admin.Models;

namespace NopStation.Plugin.Pickup.PickupInStoreAdvance.Areas.Admin.Factories
{
    public interface IPickupInStoreDeliveryManageModelFactory
    {
        Task<PickupInStoreDeliveryManageListModel> PreparePickupInStoreDeliveryManageListModelAsync(PickupInStoreDeliveryManageSearchModel searchModel);
        PickupInStoreDeliveryManageSearchModel PreparePickupInStoreDeliveryManageSearchModel(PickupInStoreDeliveryManageSearchModel searchModel);
    }
}