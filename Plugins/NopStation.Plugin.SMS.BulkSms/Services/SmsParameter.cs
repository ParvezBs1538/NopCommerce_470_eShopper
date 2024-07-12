using Newtonsoft.Json;

namespace NopStation.Plugin.SMS.BulkSms.Services
{
    public class SmsParameter
    {
        [JsonProperty("to")]
        public string To { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }
    }
}
