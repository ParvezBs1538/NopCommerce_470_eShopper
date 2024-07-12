using System.Threading.Tasks;
using Nop.Core.Domain.Orders;
using Nop.Services.Payments;
using NopStation.Plugin.Payments.SSLCommerz.Domains;
using NopStation.Plugin.Payments.SSLCommerz.Sevices.Results;

namespace NopStation.Plugin.Payments.SSLCommerz.Sevices
{
    public interface ISSLCommerzManager
    {
        Task<PaymentInitResult> GetGatewayRedirectUrlAsync(Order order);

        Task<PaymentValidationResult> ValidatePaymentAsync(Order order);

        Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest refundPaymentRequest);

        Task<RefundValidationResult> VerifyRefundAsync(Refund refund, Order order);
    }
}