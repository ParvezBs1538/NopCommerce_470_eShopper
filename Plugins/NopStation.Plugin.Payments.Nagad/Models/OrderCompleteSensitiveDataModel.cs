using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.Nagad.Models
{
    public class OrderCompleteSensitiveDataModel
    {
        [JsonProperty("datetime")]
        public string DateTime { get; set; }

        [JsonProperty("merchantId")]
        public string MerchantId { get; set; }

        [JsonProperty("orderId")]
        public string OrderId { get; set; }

        [JsonProperty("currencyCode")]
        public string CurrencyCode { get; set; }

        [JsonProperty("amount")]
        public string Amount { get; set; }

        [JsonProperty("challenge")]
        public string Challenge { get; set; }
    }
}
