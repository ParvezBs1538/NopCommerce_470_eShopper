using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.Paystack.Models.Request
{
    public class RefundRequestModel
    {
        [JsonProperty("amount")]
        public int Amount { get; set; }

        [JsonProperty("transaction")]
        public string Transaction { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("customer_note")]
        public string CustomerNote { get; set; }

        [JsonProperty("merchant_note")]
        public string MerchantNote { get; set; }
    }
}
