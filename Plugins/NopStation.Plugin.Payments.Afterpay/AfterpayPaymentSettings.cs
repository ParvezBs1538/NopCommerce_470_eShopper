using Nop.Core.Configuration;

namespace NopStation.Plugin.Payments.Afterpay
{
    public class AfterpayPaymentSettings : ISettings
    {
        public bool UseSandbox { get; set; }
        public bool Debug { get; set; }
        public string MerchantId { get; set; }
        public string MerchantKey { get; set; }
        public int MinimumAmount { get; set; }
        public int MaximumAmount { get; set; }
    }
}