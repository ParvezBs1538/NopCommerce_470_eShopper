using System.Collections.Generic;
using Nop.Core.Configuration;

namespace NopStation.Plugin.Payments.StripeKonbini
{
    public class StripeKonbiniPaymentSettings : ISettings
    {
        public StripeKonbiniPaymentSettings()
        {
            SupportedCurrencyCodes = new List<string>();
        }

        public bool AdditionalFeePercentage { get; set; }

        public decimal AdditionalFee { get; set; }

        public string ApiKey { get; set; }

        public string PublishableKey { get; set; }

        public string WebhookSecret { get; set; }

        public bool SendOrderInfoToStripe { get; set; }

        public List<string> SupportedCurrencyCodes { get; set; }
    }
}