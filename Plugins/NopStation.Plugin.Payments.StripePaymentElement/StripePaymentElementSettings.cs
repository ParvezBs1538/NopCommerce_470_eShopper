using Nop.Core.Configuration;

namespace NopStation.Plugin.Payments.StripePaymentElement;

public class StripePaymentElementSettings : ISettings
{
    public TransactionMode TransactionMode { get; set; }

    public string Theme { get; set; }

    public string Layout { get; set; }

    public bool AdditionalFeePercentage { get; set; }

    public decimal AdditionalFee { get; set; }

    public string SecretKey { get; set; }

    public string PublishableKey { get; set; }

    public bool EnableLogging { get; set; }
}
