namespace NopStation.Plugin.Payments.StripePaymentElement.Services;

public static class StripePaymentElementManager
{
    #region Methods

    public static bool IsConfigured(StripePaymentElementSettings settings)
    {
        return !string.IsNullOrEmpty(settings?.SecretKey) && !string.IsNullOrEmpty(settings?.PublishableKey);
    }

    #endregion
}
