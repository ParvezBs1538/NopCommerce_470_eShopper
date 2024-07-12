using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.NetsEasy.Models
{
    public class CancelPaymentModel
    {
        [JsonProperty("amount")]
        public int Amount { get; set; }
    }
}
