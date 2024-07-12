using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace NopStation.Plugin.Payments.Nagad.Models.Response
{
    public class PaymentInitializeResponseModel
    {
        [JsonProperty("sensitiveData")]
        public string SensitiveData { get; set; }
        [JsonProperty("signature")]
        public string Signature { get; set; }
    }
}
