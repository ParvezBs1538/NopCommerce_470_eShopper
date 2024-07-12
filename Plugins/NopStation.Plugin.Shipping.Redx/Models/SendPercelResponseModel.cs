using System.Collections.Generic;
using Newtonsoft.Json;

namespace NopStation.Plugin.Shipping.Redx.Models
{
    public class SendPercelResponseModel
    {
        public SendPercelResponseModel()
        {
            ValidationErrors = new List<Dictionary<string, string>>();
        }

        [JsonProperty("Response_Code")]
        public int ResponseCode { get; set; }

        public string Message { get; set; }

        [JsonProperty("Tracking_Id")]
        public string TrackingId { get; set; }

        [JsonProperty("Validation_Errors")]
        public List<Dictionary<string, string>> ValidationErrors { get; set; }
    }
}