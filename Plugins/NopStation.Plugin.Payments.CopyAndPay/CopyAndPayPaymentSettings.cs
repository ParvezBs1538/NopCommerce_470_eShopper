using Nop.Core.Configuration;

namespace NopStation.Plugin.Payments.CopyAndPay
{
    public class CopyAndPayPaymentSettings : ISettings
    {
        public bool AdditionalFeePercentage { get; set; }

        public decimal AdditionalFee { get; set; }

        public string EntityId { get; set; }

        public string MadaEntityId { get; set; }

        public string TestMode { get; set; }

        public string AuthorizationKey { get; set; }

        public string APIUrl { get; set; }
    }
}
