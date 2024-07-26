using Newtonsoft.Json;

namespace Nop.Plugin.Payments.GemVisaPayment.Models
{
    public class PurchaseResponseBody
    {
        [JsonProperty("merchantId")]
        public string MerchantId { get; set; }

        [JsonProperty("merchantReference")]
        public string MerchantReference { get; set; }

        [JsonProperty("amount")]
        public double Amount { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("result")]
        public string Result { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("redirectUrl")]
        public string RedirectUrl { get; set; }
    }
}

