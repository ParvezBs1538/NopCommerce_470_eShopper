
using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.Nagad.Models.Request
{
    public class PaymentInitializeRequestModel
    {
        [JsonProperty("dateTime")]
        public string DateTime { get; set; }

        [JsonProperty("sensitiveData")]
        public string SensitiveData { get; set; }

        [JsonProperty("signature")]
        public string Signature { get; set; }
    }
}
