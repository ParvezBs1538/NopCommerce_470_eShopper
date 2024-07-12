using System.Threading.Tasks;
using Nop.Core.Domain.Shipping;
using NopStation.Plugin.Widgets.DeliveryScheduler.Domains;
using NopStation.Plugin.Widgets.DeliveryScheduler.Models;

namespace NopStation.Plugin.Widgets.DeliveryScheduler.Factories
{
    public interface IDeliverySchedulerModelFactory
    {
        Task<DeliverySlotDetailsModel> PrepareDeliverySlotDetailsModel(ShippingMethod shippingMethod);

        Task<OrderDeliveryDetailsModel> PreparedOrderDeliverySlotModelAsync(OrderDeliverySlot orderDeliverySlot);
    }
}