using Nop.Core.Configuration;

namespace NopStation.Plugin.Payments.Dmoney
{
    public class DmoneyPaymentSettings : ISettings
    {
        public string GatewayUrl { get; set; }

        public string TransactionVerificationUrl { get; set; }

        public string OrganizationCode { get; set; }

        public string Password { get; set; }

        public string SecretKey { get; set; }

        public string BillerCode { get; set; }

        public string Description { get; set; }
    }
}
