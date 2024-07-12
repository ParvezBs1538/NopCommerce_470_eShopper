using System.Threading.Tasks;
using NopStation.Plugin.Payments.BlueSnapHosted.Models;
using NopStation.Plugin.Payments.BlueSnapHosted.Models.SubscriptionCharges;
using Nop.Services.Payments;

namespace NopStation.Plugin.Payments.BlueSnapHosted.Services
{
    public interface IBlueSnapServices
    {
        Task<string> GetTokenAsync();

        Task<PlanResponse> CreatePlanAsync(ProcessPaymentRequest processPaymentRequest);

        Task<PaymentResponse> BlueSnapPaymentAPIAsync(ProcessPaymentRequest processPaymentRequest);

        Task<RefundResponse> BlueSnapRefundAPIAsync(RefundPaymentRequest refundPaymentRequest);

        Task<SubscriptionResponse> BlueSnapSubscriptionAPIAsync(int planId, ProcessPaymentRequest processPaymentRequest);

        Task<string> BlueSnapSubscriptionTransactionIdAsync(string subscriptionId);

        Task<SubscriptionCharges> BlueSnapChargesForSubscriptionIdAsync(string subscriptionId);

        Task<bool> MakeChargeForSubscriptionIdAsync(string subscriptionId);

        Task<string> CancelRecurringPaymentAsync(CancelRecurringPaymentRequest cancelPaymentRequest);
    }
}
