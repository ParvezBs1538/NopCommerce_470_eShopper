using System.Collections.Generic;
using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.Quickstream.Models
{
    public class Link
    {
        [JsonProperty("rel")]
        public string Rel { get; set; }
        [JsonProperty("href")]
        public string Href { get; set; }
        [JsonProperty("requestMethod")]
        public string RequestMethod { get; set; }
    }

    public class CreditCard
    {
        [JsonProperty("cardholderName")]
        public string CardholderName { get; set; }
        [JsonProperty("cardNumber")]
        public string CardNumber { get; set; }
        [JsonProperty("expiryDateMonth")]
        public string ExpiryDateMonth { get; set; }
        [JsonProperty("expiryDateYear")]
        public string ExpiryDateYear { get; set; }
        [JsonProperty("cardScheme")]
        public string CardScheme { get; set; }
        [JsonProperty("cardType")]
        public string CardType { get; set; }
        [JsonProperty("surchargePercentage")]
        public string SurchargePercentage { get; set; }
        [JsonProperty("maskedCardNumber4Digits")]
        public string MaskedCardNumber4Digits { get; set; }
    }

    public class SingleUseTokenResponse
    {
        [JsonProperty("links")]
        public List<Link> Links { get; set; }
        [JsonProperty("singleUseTokenId")]
        public string SingleUseTokenId { get; set; }
        [JsonProperty("accountType")]
        public string AccountType { get; set; }
        [JsonProperty("creditCard")]
        public CreditCard CreditCard { get; set; }
    }
}
