using System.Threading.Tasks;
using Nop.Core.Domain.Orders;
using NopStation.Plugin.Payments.Afterpay.Models;

namespace NopStation.Plugin.Payments.Afterpay.Services
{
    public interface IAfterpayPaymentService
    {
        Task<AuthResponse> GetCancelPaymentResponseAsync(string orderToken);

        Task<AuthResponse> GetPaymentStatusAsync(string orderToken);

        Task<PaymentUrlResponse> GetResponseAsync(PaymentUrlRequest paymentUrlRequest);

        Task<PaymentUrlRequest> GeneratePaymentUrlRequestAsync(Order order);

        Task<AuthResponse> GetCapturedResponseAsync(Order order);

        Task<string> GetAfterpayOrderIdByTokenAsync(string token);

        Task<RefundResponse> RefundPaymentAsync(string token, decimal amount, int orderId);
    }
}
