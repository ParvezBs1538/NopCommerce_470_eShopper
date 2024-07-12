using Nop.Core.Configuration;

namespace NopStation.Plugin.Payments.SSLCommerz
{
    public class SSLCommerzPaymentSettings : ISettings
    {
        public string DescriptionText { get; set; }

        public bool UseSandbox { get; set; }

        public string BusinessEmail { get; set; }

        public bool PassProductNamesAndTotals { get; set; }

        public decimal AdditionalFee { get; set; }

        public bool AdditionalFeePercentage { get; set; }

        public string StoreID { get; set; }

        public string Password { get; set; }
    }
}
