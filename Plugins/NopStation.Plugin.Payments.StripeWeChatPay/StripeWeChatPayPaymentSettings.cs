﻿using System.Collections.Generic;
using Nop.Core.Configuration;

namespace NopStation.Plugin.Payments.StripeWeChatPay
{
    public class StripeWeChatPayPaymentSettings : ISettings
    {
        public StripeWeChatPayPaymentSettings()
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