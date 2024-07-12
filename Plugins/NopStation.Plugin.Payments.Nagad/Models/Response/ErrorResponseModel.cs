using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.Nagad.Models.Response
{
    public class ErrorResponseModel
    {
        [JsonProperty("reason")]
        public string Reason { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
