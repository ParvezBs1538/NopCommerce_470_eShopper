using Newtonsoft.Json;

namespace Nop.Plugin.Payments.AfterpayPayment.Models
{
    public class RefundRequest
    {
        [JsonProperty("amount")]
        public Amount TotalAmount { get; set; }

        [JsonProperty("merchantReference")]
        public string MerchantReference { get; set; }

        [JsonProperty("requestId")]
        public string RefundRequestId { get; set; }
    }
}
