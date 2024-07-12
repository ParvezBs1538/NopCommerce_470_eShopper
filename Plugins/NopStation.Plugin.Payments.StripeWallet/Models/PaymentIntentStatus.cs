

namespace NopStation.Plugin.Payments.StripeWallet.Models
{
    public enum PaymentIntentStatus
    {
        succeeded,
        requires_capture,
        canceled
    }
}
