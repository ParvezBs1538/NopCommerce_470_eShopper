using System;
using Newtonsoft.Json;

namespace NopStation.Plugin.SMS.TeleSign.Services.Responses
{
    public class TelesignResponse
    {
        public TelesignResponse()
        {
            Status = new Status();
        }

        [JsonProperty("reference_id")]
        public string ReferenceId { get; set; }

        [JsonProperty("status")]
        public Status Status { get; set; }
    }

    public class Status
    {
        [JsonProperty("code")]
        public int Code { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("updated_on")]
        public DateTime UpdatedOn { get; set; }
    }
}
