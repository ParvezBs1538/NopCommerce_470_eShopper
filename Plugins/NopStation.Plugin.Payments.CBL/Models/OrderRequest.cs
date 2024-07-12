using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.CBL.Models
{
    public class OrderRequest
    {
        [JsonProperty("orderID")]
        public string OrderId { get; set; }

        [JsonProperty("sessionID")]
        public string SessionId { get; set; }

        [JsonProperty("merchantID")]
        public string MerchantId { get; set; }

        [JsonProperty("userName")]
        public string UserName { get; set; }

        [JsonProperty("passWord")]
        public string Password { get; set; }

        [JsonProperty("secureToken")]
        public string SecureToken { get; set; }
    }
}
