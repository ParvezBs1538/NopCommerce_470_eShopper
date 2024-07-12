using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.bKash.Models
{
    public class GrantTokenRequest
    {
        [JsonProperty(PropertyName = "app_key")]
        public string AppKey { get; set; }

        [JsonProperty(PropertyName = "app_secret")]
        public string AppSecret { get; set; }
    }
}
