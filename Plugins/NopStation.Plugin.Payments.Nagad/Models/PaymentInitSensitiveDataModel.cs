using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.Nagad.Models
{
    public class PaymentInitSensitiveDataModel
    {
        [JsonProperty("merchantId")]
        public string MerchantId { get; set; }

        [JsonProperty("datetime")]
        public string DateTime { get; set; }

        [JsonProperty("orderId")]
        public string OrderId { get; set; }

        [JsonProperty("challenge")]
        public string Challenge { get; set; }
    }
}
