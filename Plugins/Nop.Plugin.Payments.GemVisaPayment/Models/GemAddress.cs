using Newtonsoft.Json;

namespace Nop.Plugin.Payments.GemVisaPayment.Models
{
    public class GemAddress
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("line1")]
        public string Line1 { get; set; }


        [JsonProperty("line2")]
        public string Line2 { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("postCode")]
        public string PostCode { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("countryCode")]
        public string CountryCode { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }
    }
}
