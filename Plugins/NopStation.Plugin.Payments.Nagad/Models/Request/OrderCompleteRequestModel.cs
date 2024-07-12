using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.Nagad.Models.Request
{
    public class OrderCompleteRequestModel
    {
        [JsonProperty("sensitiveData")]
        public string SensitiveData { get; set; }

        [JsonProperty("signature")]
        public string Signature { get; set; }

        [JsonProperty("merchantCallbackURL")]
        public string MerchantCallbackURL { get; set; }
    }
}
