using System.Collections.Generic;
using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.Quickstream.Models
{
    public class SurChargeResponseBody
    {
        [JsonProperty("links")]
        public List<Link> Links { get; set; }

        [JsonProperty("data")]
        public List<Card> Cards { get; set; }
    }
}
