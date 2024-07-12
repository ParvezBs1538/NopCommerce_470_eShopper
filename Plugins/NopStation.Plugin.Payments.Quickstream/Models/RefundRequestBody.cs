using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.Quickstream.Models
{
    public class RefundRequestBody
    {
        [JsonProperty("originalReceiptNumber")]
        public string OriginalReceiptNumber { get; set; }

        [JsonProperty("transactionType")]
        public string TransactionType { get; set; }

        [JsonProperty("principalAmount")]
        public double PrincipalAmount { get; set; }

        [JsonProperty("customerReferenceNumber")]
        public string CustomerReferenceNumber { get; set; }

        [JsonProperty("paymentReferenceNumber")]
        public string PaymentReferenceNumber { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }
    }
}
