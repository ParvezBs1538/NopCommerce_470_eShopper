using System.Collections.Generic;
using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.Nagad.Models
{
    public class PaymentDetails
    {
        [JsonProperty("merchantId")]
        public string MerchantId { get; set; }
        [JsonProperty("orderId")]
        public string OrderId { get; set; }
        [JsonProperty("paymentRefId")]
        public string PaymentRefId { get; set; }
        [JsonProperty("amount")]
        public string Amount { get; set; }
        [JsonProperty("clientMobileNo")]
        public string ClientMobileNo { get; set; }
        [JsonProperty("merchantMobileNo")]
        public string MerchantMobileNo { get; set; }
        [JsonProperty("orderDateTime")]
        public string OrderDateTime { get; set; }
        [JsonProperty("issuerPaymentDateTime")]
        public string IssuerPaymentDateTime { get; set; }
        [JsonProperty("issuerPaymentReferenceNo")]
        public string IssuerPaymentReferenceNo { get; set; }
        [JsonProperty("additionalMerchantInfo")]
        public Dictionary<string, string> AdditionalMerchantInfo { get; set; }
        [JsonProperty("status")]
        public NagadStatus Status { get; set; }
        [JsonProperty("statusCode")]
        public string StatusCode { get; set; }
        [JsonProperty("cancelIssuerDateTime")]
        public string CancelIssuerDateTime { get; set; }
        [JsonProperty("cancelIssuerRefNo")]
        public string CancelIssuerRefNo { get; set; }
    }
}