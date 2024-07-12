using Nop.Core.Configuration;

namespace NopStation.Plugin.Payments.CBL
{
    public class CBLPaymentSettings : ISettings
    {
        public bool UseSandbox { get; set; }
        public bool Debug { get; set; }
        public string MerchantId { get; set; }
        public string MerchantUsername { get; set; }
        public string MerchantPassword { get; set; }
    }
}
