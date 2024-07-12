using System.Collections.Generic;
using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.Quickstream.Models
{
    public class Card
    {
        [JsonProperty("links")]
        public List<Link> Links { get; set; }

        [JsonProperty("cardScheme")]
        public string CardScheme { get; set; }

        [JsonProperty("cardType")]
        public string CardType { get; set; }

        [JsonProperty("surchargePercentage")]
        public string SurchargePercentage { get; set; }
    }
}
