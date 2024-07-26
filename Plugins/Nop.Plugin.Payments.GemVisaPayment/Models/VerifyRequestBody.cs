using Newtonsoft.Json;

namespace Nop.Plugin.Payments.GemVisaPayment.Models
{
    public class VerifyRequestBody
    {
        [JsonProperty("gatewayReference")]
        public string GatewayReference { get; set; }

        [JsonProperty("transactionReference")]
        public string TransactionReference { get; set; }

        [JsonProperty("merchantReference")]
        public string MerchantReference { get; set; }
    }
}
