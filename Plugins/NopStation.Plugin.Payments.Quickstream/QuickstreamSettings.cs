using Nop.Core.Configuration;

namespace NopStation.Plugin.Payments.Quickstream
{
    public class QuickstreamSettings : ISettings
    {
        public bool UseSandbox { get; set; }

        public string PublishableApiKey { get; set; }

        public string SecretApiKey { get; set; }

        public string SupplierBusinessCode { get; set; }

        public string CommunityCode { get; set; }

        public string IpAddress { get; set; }
    }
}