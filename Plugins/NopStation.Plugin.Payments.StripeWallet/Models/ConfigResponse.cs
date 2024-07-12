using Newtonsoft.Json;
namespace NopStation.Plugin.Payments.StripeWallet.Models
{
    public class ConfigResponse
    {
        [JsonProperty("publishableKey")]
        public string PublishableKey { get; set; }
    }
}