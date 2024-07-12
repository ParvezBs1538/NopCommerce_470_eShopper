using Nop.Core.Configuration;

namespace NopStation.Plugin.Payments.Flutterwave
{
    public class FlutterwavePaymentSettings : ISettings
    {
        public bool UseSandbox { get; set; }

        public string SecretKey { get; set; }

        public string PublicKey { get; set; }

        public string EncryptionKey { get; set; }

        public string Description { get; set; }

        public decimal AdditionalFee { get; set; }

        public bool AdditionalFeePercentage { get; set; }
    }
}
