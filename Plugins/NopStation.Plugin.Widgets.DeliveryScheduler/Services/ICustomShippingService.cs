using System.Threading.Tasks;
using Nop.Core.Domain.Shipping;

namespace NopStation.Plugin.Widgets.DeliveryScheduler.Services
{
    public interface ICustomShippingService
    {
        Task<ShippingMethod> GetShippingMethodByNameAsync(string shippingMethodName);
    }
}