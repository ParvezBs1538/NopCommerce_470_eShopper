using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.Dmoney.Models
{
    public class TransactionStatusResponse
    {
        public TransactionStatusResponse()
        {
            Error = new ErrorModel();
            Data = new DataModel();
        }

        [JsonProperty(PropertyName = "data")]
        public DataModel Data { get; set; }

        [JsonProperty(PropertyName = "requestId")]
        public string RequestId { get; set; }

        [JsonProperty(PropertyName = "status")]
        public int StatusCode { get; set; }

        [JsonProperty(PropertyName = "error")]
        public ErrorModel Error { get; set; }
    }

    public class ErrorModel
    {
        [JsonProperty(PropertyName = "code")]
        public string ErrorCode { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }
    }

    public class DataModel
    {
        [JsonProperty(PropertyName = "merchantWalletNo")]
        public string MerchantWalletNumber { get; set; }

        [JsonProperty(PropertyName = "customerWalletNo")]
        public string CustomerWalletNumber { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "transactionType")]
        public string TransactionType { get; set; }

        [JsonProperty(PropertyName = "billOrInvoiceNo")]
        public string BillOrInvoiceNumber { get; set; }

        [JsonProperty(PropertyName = "transactionTime")]
        public string TransactionTime { get; set; }

        [JsonProperty(PropertyName = "statusCode")]
        public string StatusCode { get; set; }

        [JsonProperty(PropertyName = "statusMessage")]
        public string StatusMessage { get; set; }

        [JsonProperty(PropertyName = "paymentStatus")]
        public string PaymentStatus { get; set; }

        [JsonProperty(PropertyName = "transactionReferenceId")]
        public string TransactionReferenceId { get; set; }

        [JsonProperty(PropertyName = "transactionTrackingNo")]
        public string TransactionTrackingNumber { get; set; }
    }
}
