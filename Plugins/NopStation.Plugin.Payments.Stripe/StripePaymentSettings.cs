using Nop.Core.Configuration;

namespace NopStation.Plugin.Payments.Stripe
{
    public class StripePaymentSettings : ISettings
    {
        public TransactionMode TransactionMode { get; set; }

        public bool AdditionalFeePercentage { get; set; }

        public decimal AdditionalFee { get; set; }

        public string ApiKey { get; set; }
    }
}
