using Newtonsoft.Json;

namespace Nop.Plugin.Payments.AfterpayPayment.Models
{
    public class ConfigurationResponse
    {
        [JsonProperty("minimumAmount")]
        public Amount MinimumAmount { get; set; }

        [JsonProperty("maximumAmount")]
        public Amount MaximumAmount { get; set; }
    }
}
