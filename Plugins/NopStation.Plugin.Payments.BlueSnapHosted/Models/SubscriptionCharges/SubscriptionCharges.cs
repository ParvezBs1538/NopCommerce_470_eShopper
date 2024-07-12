using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.BlueSnapHosted.Models.SubscriptionCharges
{
    public class SubscriptionCharges
    {
        [JsonProperty("lastPage")]
        public bool LastPage { get; set; }
        [JsonProperty("charges")]
        public List<Charge> Charges { get; set; }
    }
}
