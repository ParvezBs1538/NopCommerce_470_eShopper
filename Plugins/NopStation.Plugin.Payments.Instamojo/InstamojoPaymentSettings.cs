using Nop.Core.Configuration;

namespace NopStation.Plugin.Payments.Instamojo
{
    public class InstamojoPaymentSettings : ISettings
    {
        public bool UseSandbox { get; set; }

        public string PrivateApiKey { get; set; }

        public string PrivateAuthToken { get; set; }

        public string PrivateSalt { get; set; }

        public string Description { get; set; }

        public decimal AdditionalFee { get; set; }

        public bool AdditionalFeePercentage { get; set; }

        public bool EnableSendSMS { get; set; }

        public bool EnableSendEmail { get; set; }

        public string PhoneNumberRegex { get; set; }
    }
}
