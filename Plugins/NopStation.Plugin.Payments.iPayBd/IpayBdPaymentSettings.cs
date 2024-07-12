using Nop.Core.Configuration;

namespace NopStation.Plugin.Payments.iPayBd
{
    public class IpayBdPaymentSettings : ISettings
    {
        public string DescriptionText { get; set; }

        public bool AdditionalFeePercentage { get; set; }

        public decimal AdditionalFee { get; set; }

        public string ApiKey { get; set; }

        public bool Sandbox { get; set; }
    }
}