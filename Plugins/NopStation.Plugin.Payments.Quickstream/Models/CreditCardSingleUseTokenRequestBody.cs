using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.Quickstream.Models
{
    public class CreditCardSingleUseTokenRequestBody
    {
        [JsonProperty("supplierBusinessCode")]
        public string SupplierBusinessCode { get; set; }

        [JsonProperty("accountType")]
        public string AccountType { get; set; }

        [JsonProperty("cardholderName")]
        public string CardholderName { get; set; }

        [JsonProperty("cardNumber")]
        public string CardNumber { get; set; }

        [JsonProperty("expiryDateMonth")]
        public string ExpiryDateMonth { get; set; }

        [JsonProperty("expiryDateYear")]
        public string ExpiryDateYear { get; set; }

        [JsonProperty("cvn")]
        public string Cvn { get; set; }
    }
}
