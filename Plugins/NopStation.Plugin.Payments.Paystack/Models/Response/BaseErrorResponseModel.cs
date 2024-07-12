using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.Paystack.Models.Response
{
    public class BaseErrorResponseModel
    {
        [JsonProperty("error")]
        public ErrorModel Error { get; set; }

        public class ErrorModel
        {
            [JsonProperty("code")]
            public string Code { get; set; }

            [JsonProperty("description")]
            public string Description { get; set; }

            [JsonProperty("source")]
            public string Source { get; set; }

            [JsonProperty("step")]
            public string Step { get; set; }

            [JsonProperty("reason")]
            public string Reason { get; set; }

            [JsonProperty("metadata")]
            public MetadataModel Metadata { get; set; }
        }

        public class MetadataModel
        {
        }
    }
}
