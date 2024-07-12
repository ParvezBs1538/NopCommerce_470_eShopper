namespace NopStation.Plugin.Payments.StripePaymentElement.Models;

public enum PaymentIntentStatus
{
    succeeded,
    requires_capture,
    canceled
}
