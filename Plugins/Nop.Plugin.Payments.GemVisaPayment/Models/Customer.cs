using Newtonsoft.Json;

namespace Nop.Plugin.Payments.GemVisaPayment.Models
{
    public class Customer
    {
        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }
    }
}
