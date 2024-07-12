using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.Razorpay.Models.Request
{
    public class RefundRequestModel
    {
        [JsonProperty("amount")]
        public int Amount { get; set; }

        [JsonProperty("speed")]
        public string Speed { get; set; }

        [JsonProperty("receipt")]
        public string Receipt { get; set; }
    }
}
