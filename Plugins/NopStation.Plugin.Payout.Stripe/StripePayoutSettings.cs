using Nop.Core.Configuration;

namespace NopStation.Plugin.Payout.Stripe
{
    public class StripePayoutSettings : ISettings
    {
        public bool Active { get; set; }
        public bool UseSandbox { get; set; }
        public string SecretKey { get; set; }
    }
}