using System.Collections.Generic;
using Newtonsoft.Json;

namespace Nop.Plugin.Payments.GemVisaPayment.Models
{
    public class PurchaseRequestBody
    {
        [JsonProperty("merchantId")]
        public string MerchantId { get; set; }

        [JsonProperty("isTest")]
        public bool IsTest { get; set; }

        [JsonProperty("merchantReference")]
        public string MerchantReference { get; set; }

        [JsonProperty("amount")]
        public double Amount { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("promotionReference")]
        public string PromotionReference { get; set; }

        [JsonProperty("customer")]
        public Customer Customer { get; set; }

        [JsonProperty("shippingAddress")]
        public GemAddress ShippingAddress { get; set; }

        [JsonProperty("billingAddress")]
        public GemAddress BillingAddress { get; set; }

        [JsonProperty("orderLines")]
        public List<OrderLine> OrderLines { get; set; }

        [JsonProperty("merchantUrls")]
        public MerchantUrls MerchantUrls { get; set; }

        [JsonProperty("totalShippingAmount")]
        public double TotalShippingAmount { get; set; }

        [JsonProperty("platformType")]
        public string PlatformType { get; set; }

        [JsonProperty("platformVersion")]
        public string PlatformVersion { get; set; }

        [JsonProperty("requiresShipping")]
        public bool RequiresShipping { get; set; }
    }


}
