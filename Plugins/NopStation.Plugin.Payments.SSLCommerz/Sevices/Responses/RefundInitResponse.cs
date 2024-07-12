using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.SSLCommerz.Sevices.Responses
{
    public class RefundInitResponse
    {
        [JsonProperty("APIConnect")]
        public string APIConnect { get; set; }

        [JsonProperty("bank_tran_id")]
        public string BankTransactionId { get; set; }

        [JsonProperty("trans_id")]
        public string TransactionId { get; set; }

        [JsonProperty("refund_ref_id")]
        public string RefundRefrenceId { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("errorReason")]
        public string ErrorReason { get; set; }
    }
}
