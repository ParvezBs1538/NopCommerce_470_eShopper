using System.Collections.Generic;
using Nop.Core.Configuration;

namespace NopStation.Plugin.Payments.StripeSofort
{
    public class StripeSofortPaymentSettings : ISettings
    {
        public StripeSofortPaymentSettings()
        {
            SupportedCurrencyCodes = new List<string>();
            SupportedCountryIds = new List<int>();
        }

        public string DescriptionText { get; set; }

        public bool AdditionalFeePercentage { get; set; }

        public decimal AdditionalFee { get; set; }

        public string ApiKey { get; set; }

        public string PublishableKey { get; set; }

        public string WebhookSecret { get; set; }

        public List<string> SupportedCurrencyCodes { get; set; }

        public List<int> SupportedCountryIds { get; set; }
    }
}