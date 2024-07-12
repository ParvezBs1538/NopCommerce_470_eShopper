using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.Afterpay.Models
{
    public class ConfigurationResponse
    {
        [JsonProperty("minimumAmount")]
        public PaymentAfterpayModel MinimumAmount { get; set; }

        [JsonProperty("maximumAmount")]
        public PaymentAfterpayModel MaximumAmount { get; set; }
    }
}
