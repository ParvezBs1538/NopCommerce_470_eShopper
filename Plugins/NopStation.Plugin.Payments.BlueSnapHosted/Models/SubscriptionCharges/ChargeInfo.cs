using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.BlueSnapHosted.Models.SubscriptionCharges
{
    public class ChargeInfo
    {
        [JsonProperty("fromDate")]
        public DateTime FromDate { get; set; }
        [JsonProperty("toDate")]
        public DateTime ToDate { get; set; }
        [JsonProperty("chargeType")]
        public string ChargeType { get; set; }
    }
}
