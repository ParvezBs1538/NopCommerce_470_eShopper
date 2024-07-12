using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.Nagad.Models.Response
{
    public class OrderCompleteResponseModel
    {
        [JsonProperty("callBackUrl")]
        public string CallBackUrl { get; set; }
        [JsonProperty("status")]
        public string Status { get; set; }
    }
}
