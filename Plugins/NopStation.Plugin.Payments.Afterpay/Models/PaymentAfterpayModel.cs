using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.Afterpay.Models
{
    public class PaymentAfterpayModel
    {
        [JsonProperty("amount")]
        public string PaymentAmount { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }
    }
}
