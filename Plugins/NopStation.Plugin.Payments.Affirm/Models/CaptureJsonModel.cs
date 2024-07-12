using System;
using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.Affirm.Models
{
    public class CaptureJsonModel
    {
        [JsonProperty("fee")]
        public int Fee { get; set; }

        [JsonProperty("created")]
        public DateTime Created { get; set; }

        [JsonProperty("order_id")]
        public string OrderId { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("amount")]
        public int Amount { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("transaction_id")]
        public string TransactionId { get; set; }
    }
}
