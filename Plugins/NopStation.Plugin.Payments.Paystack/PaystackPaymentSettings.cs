using System.Collections.Generic;
using Nop.Core.Configuration;

namespace NopStation.Plugin.Payments.Paystack
{
    public class PaystackPaymentSettings : ISettings
    {
        public PaystackPaymentSettings()
        {
            Channels = new List<string>();
        }

        public string PublicKey { get; set; }

        public string SecretKey { get; set; }

        public List<string> Channels { get; set; }

        public string Description { get; set; }

        public decimal AdditionalFee { get; set; }

        public bool AdditionalFeePercentage { get; set; }

        public List<string> Currencies { get; set; }
    }
}
