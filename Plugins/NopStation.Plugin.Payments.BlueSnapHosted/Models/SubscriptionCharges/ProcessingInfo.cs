using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.BlueSnapHosted.Models.SubscriptionCharges
{
    public class ProcessingInfo
    {
        [JsonProperty("processingStatus")]
        public string ProcessingStatus { get; set; }
        [JsonProperty("authorizationCode")]
        public string AuthorizationCode { get; set; }
    }
}
