using Nop.Core.Configuration;

namespace NopStation.Plugin.Payments.AmazonPay
{
    public class AmazonPaySettings : ISettings
    {
        public string DescriptionText { get; set; }

        public bool UseSandbox { get; set; }

        public bool AdditionalFeePercentage { get; set; }

        public decimal AdditionalFee { get; set; }

        public string MerchantId { get; set; }

        public string PublicKeyId { get; set; }

        public string PrivateKey { get; set; }

        public string NoteToBuyer { get; set; }

        public string ButtonColor { get; set; }

        public string StoreId { get; set; }

        public int RegionId { get; set; }
    }
}
