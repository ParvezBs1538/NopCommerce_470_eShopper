using System;
using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.SSLCommerz.Sevices.Responses
{
    public class RefundValidationResponse
    {
        [JsonProperty("APIConnect")]
        public string APIConnect { get; set; }

        [JsonProperty("bank_tran_id")]
        public string BankTransactionId { get; set; }

        [JsonProperty("tran_id")]
        public string TranId { get; set; }

        [JsonProperty("initiated_on")]
        public DateTime InitiatedOn { get; set; }

        [JsonProperty("refunded_on")]
        public string RefundedOn { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("refund_ref_id")]
        public string RefundRefrenceId { get; set; }
    }
}
