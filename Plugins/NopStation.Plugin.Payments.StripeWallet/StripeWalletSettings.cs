using Nop.Core.Configuration;

namespace NopStation.Plugin.Payments.StripeWallet
{
    public class StripeWalletSettings : ISettings
    {
        public TransactionMode TransactionMode { get; set; }

        public bool AdditionalFeePercentage { get; set; }

        public decimal AdditionalFee { get; set; }

        public string SecretKey { get; set; }

        public string PublishableKey { get; set; }
    }
}
