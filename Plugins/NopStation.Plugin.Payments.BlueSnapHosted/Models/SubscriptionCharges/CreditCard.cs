using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.BlueSnapHosted.Models.SubscriptionCharges
{
    public class CreditCard
    {
        [JsonProperty("cardLastFourDigits")]
        public string CardLastFourDigits { get; set; }
        [JsonProperty("cardType")]
        public string CardType { get; set; }
        [JsonProperty("cardSubType")]
        public string CardSubType { get; set; }
        [JsonProperty("cardCategory")]
        public string CardCategory { get; set; }
        [JsonProperty("binCategory")]
        public string BinCategory { get; set; }
        [JsonProperty("cardRegulated")]
        public string CardRegulated { get; set; }
        [JsonProperty("issuingBank")]
        public string IssuingBank { get; set; }
        [JsonProperty("expirationMonth")]
        public string ExpirationMonth { get; set; }
        [JsonProperty("expirationYear")]
        public string ExpirationYear { get; set; }
        [JsonProperty("issuingCountryCode")]
        public string IssuingCountryCode { get; set; }
    }
}
