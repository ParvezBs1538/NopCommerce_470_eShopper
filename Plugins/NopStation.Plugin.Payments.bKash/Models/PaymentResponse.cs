using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.bKash.Models
{
    public class PaymentResponse
    {
        [JsonProperty(PropertyName = "paymentID")]
        public string PaymentID { get; set; }

        [JsonProperty(PropertyName = "createTime")]
        public string CreateTime { get; set; }

        [JsonProperty(PropertyName = "updateTime")]
        public string UpdateTime { get; set; }

        [JsonProperty(PropertyName = "trxID")]
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "orgName")]
        public string OrganizationName { get; set; }

        [JsonProperty(PropertyName = "orgLogo")]
        public string OrganizationLogo { get; set; }

        [JsonProperty(PropertyName = "transactionStatus")]
        public string TransactionStatus { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public string Amount { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "intent")]
        public string Intent { get; set; }

        [JsonProperty(PropertyName = "merchantInvoiceNumber")]
        public int MerchantInvoiceNumber { get; set; }

        [JsonProperty(PropertyName = "errorCode")]
        public string ErrorCode { get; set; }

        [JsonProperty(PropertyName = "errorMessage")]
        public string ErrorMessage { get; set; }
    }
}
