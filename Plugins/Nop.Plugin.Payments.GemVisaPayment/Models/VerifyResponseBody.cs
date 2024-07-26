using Newtonsoft.Json;

namespace Nop.Plugin.Payments.GemVisaPayment.Models
{
    public class VerifyResponseBody
    {
        [JsonProperty("merchantId")]
        public string MerchantId { get; set; }

        [JsonProperty("isTest")]
        public bool IsTest { get; set; }

        [JsonProperty("amount")]
        public double Amount { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("result")]
        public string Result { get; set; }

        [JsonProperty("transactionType")]
        public string TransactionType { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("gatewayReference")]
        public string GatewayReference { get; set; }

        [JsonProperty("promotionReference")]
        public string PromotionReference { get; set; }

        [JsonProperty("merchantReference")]
        public string MerchantReference { get; set; }

        [JsonProperty("transactionReference")]
        public string TransactionReference { get; set; }
    }
}
