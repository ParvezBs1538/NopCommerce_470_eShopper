using System;
using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.Afterpay.Models
{
    public class RefundResponse
    {
        [JsonProperty("amount")]
        public PaymentAfterpayModel TotalAmount { get; set; }

        [JsonProperty("merchantReference")]
        public string MerchantReference { get; set; }

        [JsonProperty("refundId")]
        public string RefundId { get; set; }

        [JsonProperty("refundedAt")]
        public DateTime RefundedAt { get; set; }
    }
}
