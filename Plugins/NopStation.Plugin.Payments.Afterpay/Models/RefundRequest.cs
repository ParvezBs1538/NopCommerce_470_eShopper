using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.Afterpay.Models
{
    public class RefundRequest
    {
        [JsonProperty("amount")]
        public PaymentAfterpayModel TotalAmount { get; set; }

        [JsonProperty("merchantReference")]
        public string MerchantReference { get; set; }

        [JsonProperty("requestId")]
        public string RefundRequestId { get; set; }
    }
}
