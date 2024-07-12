using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.Quickstream.Models
{
    public class SecurityTokenRequestBody
    {
        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("supplierBusinessCode")]
        public string SupplierBusinessCode { get; set; }

        [JsonProperty("connectionType")]
        public string ConnectionType { get; set; }

        [JsonProperty("product")]
        public string Product { get; set; }

        [JsonProperty("currencyCode")]
        public string CurrencyCode { get; set; }

        [JsonProperty("principalAmount")]
        public string PrincipleAmount { get; set; }

        [JsonProperty("paymentReference")]
        public string PaymentReference { get; set; }

        [JsonProperty("customerReferenceNumber")]
        public string CustomerReferenceNumber { get; set; }

        [JsonProperty("returnUrl")]
        public string ReturnUrl { get; set; }

        [JsonProperty("cancelUrl")]
        public string CancelUrl { get; set; }

        [JsonProperty("serverReturnUrl")]
        public string ServerReturnUrl { get; set; }

        [JsonProperty("errorEmailToAddress")]
        public string ErrorEmailToAddress { get; set; }

        [JsonProperty("receiptEmailAddress")]
        public string ReceiptEmailAddress { get; set; }
    }
}
