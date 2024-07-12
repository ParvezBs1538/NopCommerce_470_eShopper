using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.StripeWallet.Models
{
    internal class CreatePaymentIntentResponse
    {
        [JsonProperty("clientSecret")]
        public string ClientSecret { get; set; }

        [JsonProperty("result")]
        public string Result { get; internal set; }
    }
}