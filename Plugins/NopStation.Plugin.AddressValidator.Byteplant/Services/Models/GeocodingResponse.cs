using Newtonsoft.Json;

namespace NopStation.Plugin.AddressValidator.Byteplant.Services.Models
{
    public class GeocodingResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("ratelimit_remain")]
        public int RatelimitRemain { get; set; }

        [JsonProperty("ratelimit_seconds")]
        public int RatelimitSeconds { get; set; }

        [JsonProperty("cost")]
        public double Cost { get; set; }

        [JsonProperty("formattedaddress")]
        public string FormattedAddress { get; set; }

        [JsonProperty("street")]
        public string Street { get; set; }

        [JsonProperty("streetnumber")]
        public string StreetNumber { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("county")]
        public string County { get; set; }

        [JsonProperty("district")]
        public string District { get; set; }
    }
}
