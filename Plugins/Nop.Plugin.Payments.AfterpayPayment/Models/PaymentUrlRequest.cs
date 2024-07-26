using System.Collections.Generic;
using Newtonsoft.Json;

namespace Nop.Plugin.Payments.AfterpayPayment.Models
{
    public class PaymentUrlRequest
    {
        [JsonProperty("amount")]
        public Amount PaymentTotalAmount { get; set; }

        [JsonProperty("consumer")]
        public Consumer Consumer { get; set; }

        [JsonProperty("billing")]
        public AfterpayAddress Billing { get; set; }

        [JsonProperty("shipping")]
        public AfterpayAddress Shipping { get; set; }

        [JsonProperty("merchant")]
        public Merchant Merchant { get; set; }

        [JsonProperty("taxAmount")]
        public Amount TaxAmount { get; set; }

        [JsonProperty("shippingAmount")]
        public Amount ShippingAmount { get; set; }

        [JsonProperty("courier")]
        public Courier Courier { get; set; }

        [JsonProperty("items")]
        public List<Item> Items { get; set; }

        [JsonProperty("discounts")]
        public List<Discount> Discounts { get; set; }

        [JsonProperty("merchantReference")]
        public string MerchantReference { get; set; }
    }
}
