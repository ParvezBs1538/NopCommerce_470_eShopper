using Nop.Core.Configuration;

namespace NopStation.Plugin.Payments.PayHere
{
    public class PayHerePaymentSettings : ISettings
    {
        public bool UseSandbox { get; set; }

        public string MerchantId { get; set; }

        public string MerchantSecret { get; set; }

        public string Description { get; set; }

        public decimal AdditionalFee { get; set; }

        public bool AdditionalFeePercentage { get; set; }
    }
}
