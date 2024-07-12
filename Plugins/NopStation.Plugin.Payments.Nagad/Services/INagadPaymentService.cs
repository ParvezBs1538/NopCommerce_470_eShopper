using System.Threading.Tasks;
using Nop.Core.Domain.Orders;
using NopStation.Plugin.Payments.Nagad.Models;
using NopStation.Plugin.Payments.Nagad.Models.Response;

namespace NopStation.Plugin.Payments.Nagad.Services
{
    public interface INagadPaymentService
    {
        Task<PaymentInitSensitiveResponseModel> NagadPaymentInitializationAsync(Order order);
        Task<string> NagadPaymentOrderCompleteAsync(Order order, PaymentInitSensitiveResponseModel initResponse);
        Task<PaymentDetails> VerifyPaymentAsync(string referenceId, string orderGuid);
        Task<Order> GetOrderByAuthorizationTransactionId(string referenceId);
    }
}
