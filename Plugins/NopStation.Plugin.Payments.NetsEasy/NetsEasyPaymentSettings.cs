using Nop.Core.Configuration;
using NopStation.Plugin.Payments.NetsEasy.Enums;

namespace NopStation.Plugin.Payments.NetsEasy
{
    public class NetsEasyPaymentSettings : ISettings
    {
        public bool TestMode { get; set; }

        public TransactMode TransactMode { get; set; }

        public IntegrationType IntegrationType { get; set; }

        public string LimitedToCountryIds { get; set; }

        public string CheckoutKey { get; set; }

        public string SecretKey { get; set; }

        public bool AdditionalFeePercentage { get; set; }

        public decimal AdditionalFee { get; set; }

        public string CheckoutPageUrl { get; set; }

        public bool ShowB2B { get; set; }

        public bool EnableLog { get; set; }
        public bool EnsureRecurringInterval { get; set; }
    }
}
