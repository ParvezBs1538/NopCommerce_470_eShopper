using System.Threading.Tasks;
using Nop.Core.Domain.Orders;
using NopStation.Plugin.Payments.CBL.Models;

namespace NopStation.Plugin.Payments.CBL.Services
{
    public interface ICBLPaymentService
    {
        Task<PaymentUrlResponse> GetResponseAsync(PaymentUrlRequest paymentUrlRequest);
        Task<PaymentUrlRequest> GeneratePaymentUrlRequestAsync(Order order);
        Task<Order> GetTransactionByTransactionCodeAsync(string authorizationTransactionCode);
        Task<OrderResponse> GetOrderDetailsAsync(Order order, string transactionId, string sessionID);
    }
}
