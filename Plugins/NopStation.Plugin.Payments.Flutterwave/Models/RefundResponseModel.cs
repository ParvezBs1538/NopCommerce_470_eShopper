using System;
using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.Flutterwave.Models
{
    public class RefundResponseModel
    {
        public RefundResponseModel()
        {
            Refund = new RefundModel();
        }

        [JsonProperty("refund")]
        public RefundModel Refund { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("message")]
        public object Message { get; set; }

        public class RefundModel
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("payment_id")]
            public string PaymentId { get; set; }

            [JsonProperty("status")]
            public string Status { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("body")]
            public string Body { get; set; }

            [JsonProperty("refund_amount")]
            public string RefundAmount { get; set; }

            [JsonProperty("total_amount")]
            public string TotalAmount { get; set; }

            [JsonProperty("created_at")]
            public DateTime CreatedAt { get; set; }
        }
    }
}
