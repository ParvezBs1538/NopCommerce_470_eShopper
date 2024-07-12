using Nop.Core.Configuration;

namespace NopStation.Plugin.Payments.Nagad
{
    public class NagadPaymentSettings : ISettings
    {
        public string Description { get; set; }
        public bool UseSandbox { get; set; }
        public string MerchantId { get; set; }
        public string NPGPublicKey { get; set; }
        public string MSPrivateKey { get; set; }
        public bool AdditionalFeePercentage { get; set; }
        public decimal AdditionalFee { get; set; }
    }
}
