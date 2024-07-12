using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.Paykeeper.Services.Responses
{
    public class TokenResponse
    {
        [JsonProperty("token")]
        public string Token { get; set; }
    }
}
