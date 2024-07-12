using Nop.Core.Configuration;

namespace NopStation.Plugin.Payments.Paykeeper
{
    public class PaykeeperPaymentSettings : ISettings
    {
        public string DescriptionText { get; set; }

        public string SecretWord { get; set; }

        public string GatewayUrl { get; set; }

        public decimal AdditionalFee { get; set; }

        public bool AdditionalFeePercentage { get; set; }

        public string Login { get; set; }

        public string Password { get; set; }
    }
}
