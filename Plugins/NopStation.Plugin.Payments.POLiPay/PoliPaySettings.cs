using Nop.Core.Configuration;

namespace NopStation.Plugin.Payments.POLiPay
{
    public class PoliPaySettings : ISettings
    {
        public bool UseSandbox { get; set; }
        public string MerchantCode { get; set; }
        public string AuthCode { get; set; }
        public decimal AdditionalFee { get; set; }
        public bool AdditionalFeePercentage { get; set; }
    }
}
