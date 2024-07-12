using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.bKash.Models
{
    public class PaymentRequest
    {
        [JsonProperty(PropertyName = "amount")]
        public string Amount { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "intent")]
        public string Intent { get; set; }

        [JsonProperty(PropertyName = "merchantInvoiceNumber")]
        public string MerchantInvoiceNumber { get; set; }
    }
}
