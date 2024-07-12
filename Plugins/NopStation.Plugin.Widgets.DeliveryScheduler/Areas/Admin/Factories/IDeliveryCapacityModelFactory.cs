using System.Threading.Tasks;
using Nop.Core.Domain.Shipping;
using NopStation.Plugin.Widgets.DeliveryScheduler.Areas.Admin.Models;

namespace NopStation.Plugin.Widgets.DeliveryScheduler.Areas.Admin.Factories
{
    public interface IDeliveryCapacityModelFactory
    {
        Task<DeliveryCapacityConfigurationModel> PrepareConfigurationModelAsync(ShippingMethod shippingMethod);
    }
}