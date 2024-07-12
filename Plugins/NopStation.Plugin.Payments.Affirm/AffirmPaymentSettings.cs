using Nop.Core.Configuration;
using NopStation.Plugin.Payments.Affirm.Domain;

namespace NopStation.Plugin.Payments.Affirm
{
    public class AffirmPaymentSettings : ISettings
    {
        public string PublicApiKey { get; set; }

        public string PrivateApiKey { get; set; }

        public string FinancialProductKey { get; set; }

        public string MerchantName { get; set; }

        public CountryAPIMode CountryAPIMode { get; set; }

        public bool UseSandbox { get; set; }

        public TransactionMode TransactionMode { get; set; }

        public bool EnableOnShoppingCart { get; set; }

        public bool EnableOnProductDetailsPage { get; set; }

        public bool EnableOnProductBox { get; set; }

        public bool AdditionalFeePercentage { get; set; }

        public decimal AdditionalFee { get; set; }
    }
}
