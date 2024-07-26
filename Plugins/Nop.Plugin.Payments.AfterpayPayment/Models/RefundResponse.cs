using System;
using Newtonsoft.Json;

namespace Nop.Plugin.Payments.AfterpayPayment.Models
{
    public class RefundResponse
    {
        [JsonProperty("amount")]
        public Amount TotalAmount { get; set; }

        [JsonProperty("merchantReference")]
        public string MerchantReference { get; set; }

        [JsonProperty("refundId")]
        public string RefundId { get; set; }

        [JsonProperty("refundedAt")]
        public DateTime RefundedAt { get; set; }

    }
}
