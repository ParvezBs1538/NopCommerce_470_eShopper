using System.Collections.Generic;
using Nop.Core.Configuration;

namespace NopStation.Plugin.Payments.StripeGrabPay
{
    public class StripeGrabPayPaymentSettings : ISettings
    {
        public StripeGrabPayPaymentSettings()
        {
            SupportedCurrencyCodes = new List<string>();
        }

        public string DescriptionText { get; set; }

        public bool AdditionalFeePercentage { get; set; }

        public decimal AdditionalFee { get; set; }

        public string ApiKey { get; set; }

        public string PublishableKey { get; set; }

        public bool EnableWebhook { get; set; }

        public string WebhookSecret { get; set; }

        public List<string> SupportedCurrencyCodes { get; set; }
    }
}