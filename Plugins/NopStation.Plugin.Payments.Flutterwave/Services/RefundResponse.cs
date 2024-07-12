using System;
using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.Flutterwave.Services
{
    public class RefundResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("data")]
        public DataResponse Data { get; set; }

        public class DataResponse
        {
            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("account_id")]
            public int AccountId { get; set; }

            [JsonProperty("tx_id")]
            public int TxId { get; set; }

            [JsonProperty("flw_ref")]
            public string FlwRef { get; set; }

            [JsonProperty("wallet_id")]
            public int WalletId { get; set; }

            [JsonProperty("amount_refunded")]
            public int AmountRefunded { get; set; }

            [JsonProperty("status")]
            public string Status { get; set; }

            [JsonProperty("destination")]
            public string Destination { get; set; }

            [JsonProperty("meta")]
            public MetaResponse Meta { get; set; }

            [JsonProperty("created_at")]
            public DateTime CreatedAt { get; set; }
        }

        public class MetaResponse
        {
            [JsonProperty("source")]
            public string Source { get; set; }
        }
    }
}
