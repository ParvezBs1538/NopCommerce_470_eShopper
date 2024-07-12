using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace NopStation.Plugin.Shipping.Redx.Models
{
    public class TrackingResponseModel
    {
        public TrackingResponseModel()
        {
            Tracking = new List<TrackingModel>();
        }

        public List<TrackingModel> Tracking { get; set; }

        public string Message { get; set; }

        public class TrackingModel
        {
            [JsonProperty("Message_En")]
            public string MessageEnglish { get; set; }

            [JsonProperty("Message_Bn")]
            public string MessageBengali { get; set; }

            [JsonProperty("Time")]
            public DateTime Time { get; set; }
        }
    }
}