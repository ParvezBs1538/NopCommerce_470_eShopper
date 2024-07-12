using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.Paystack.Models
{
    public class CustomerModel
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("contact")]
        public string Contact { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }
    }
}
