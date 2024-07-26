using Nop.Core.Domain.Orders;
using Nop.Plugin.Payments.AfterpayPayment.Models;

namespace Nop.Plugin.Payments.AfterpayPayment.Services
{
    public interface IAfterpayPaymentService
    {
        string CancelPayment(string orderToken);
        AuthResponse GetPaymentStatus(string orderToken);
        PaymentUrlResponse GetResponse(PaymentUrlRequest paymentUrlRequest);
        PaymentUrlRequest GeneratePaymentUrlRequest(Order order);
        AuthResponse GetCapturedResponse(Order order);
        string GetAfterpayOrderIdByToken(string token);
        RefundResponse RefundPayment(string token, decimal amount, int orderId);
    }
}
