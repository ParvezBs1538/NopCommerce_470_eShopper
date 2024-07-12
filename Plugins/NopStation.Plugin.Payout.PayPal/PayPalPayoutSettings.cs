using Nop.Core.Configuration;

namespace NopStation.Plugin.Payout.PayPal
{
    public class PayPalPayoutSettings : ISettings
    {
        public bool Active { get; set; }
        public bool UseSandbox { get; set; }
        public string SecretKey { get; set; }
        public string ClientId { get; set; }
    }
}