using System.Collections.Generic;
using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.bKash.Models
{
    public class QueryResponseModel
    {
        public bool Status { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public string Amount { get; set; }

        [JsonProperty(PropertyName = "completedTime")]
        public string CompletedTime { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "customerMsisdn")]
        public string CustomerMsisdn { get; set; }

        [JsonProperty(PropertyName = "errorCode")]
        public string ErrorCode { get; set; }

        [JsonProperty(PropertyName = "errorMessage")]
        public string ErrorMessage { get; set; }

        [JsonProperty(PropertyName = "initiationTime")]
        public string InitiationTime { get; set; }

        [JsonProperty(PropertyName = "organizationShortCode")]
        public string OrganizationShortCode { get; set; }

        [JsonProperty(PropertyName = "transactionReference")]
        public string TransactionReference { get; set; }

        [JsonProperty(PropertyName = "transactionStatus")]
        public string TransactionStatus { get; set; }

        [JsonProperty(PropertyName = "transactionType")]
        public string TransactionType { get; set; }

        [JsonProperty(PropertyName = "trxID")]
        public string TransactionId { get; set; }

        [JsonProperty(PropertyName = "paymentID")]
        public string PaymentID { get; set; }

        [JsonProperty(PropertyName = "createTime ")]
        public string CreateTime { get; set; }

        [JsonProperty(PropertyName = "updateTime")]
        public string UpdateTime { get; set; }

        [JsonProperty(PropertyName = "intent")]
        public string Intent { get; set; }

        [JsonProperty(PropertyName = "merchantInvoiceNumber")]
        public string MerchantInvoiceNumber { get; set; }

        [JsonProperty(PropertyName = "refundAmount")]
        public string RefundAmount { get; set; }

        [JsonProperty(PropertyName = "organizationBalance")]
        public List<OrganizationBalance> OrganizationBalance { get; set; }
    }

    public class OrganizationBalance
    {
        [JsonProperty(PropertyName = "accountTypeName")]
        public string AccountTypeName { get; set; }

        [JsonProperty(PropertyName = "accountStatus")]
        public string AccountStatus { get; set; }

        [JsonProperty(PropertyName = "accountHolderName")]
        public string AccountHolderName { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "currentBalance")]
        public string CurrentBalance { get; set; }

        [JsonProperty(PropertyName = "availableBalance")]
        public string AvailableBalance { get; set; }

        [JsonProperty(PropertyName = "updateTime")]
        public string UpdateTime { get; set; }
    }
}
