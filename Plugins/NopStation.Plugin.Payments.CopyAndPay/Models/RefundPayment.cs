using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.CopyAndPay.Models
{
    public class RefundPayment
    {
        public RefundPayment()
        {
            Result = new ResultModel();
            ResultDetails = new ResultDetailsModel();
        }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("referencedId")]
        public string ReferencedId { get; set; }

        [JsonProperty("paymentType")]
        public string PaymentType { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("descriptor")]
        public string Descriptor { get; set; }

        [JsonProperty("result")]
        public ResultModel Result { get; set; }

        [JsonProperty("resultDetails")]
        public ResultDetailsModel ResultDetails { get; set; }

        [JsonProperty("buildNumber")]
        public string BuildNumber { get; set; }

        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }

        [JsonProperty("ndc")]
        public string Ndc { get; set; }

        public class ResultModel
        {
            [JsonProperty("code")]
            public string Code { get; set; }

            [JsonProperty("description")]
            public string Description { get; set; }
        }

        public class ResultDetailsModel
        {
            [JsonProperty("ConnectorTxID1")]
            public string ConnectorTxID1 { get; set; }

            [JsonProperty("clearingInstituteName")]
            public string ClearingInstituteName { get; set; }
        }

    }
}
