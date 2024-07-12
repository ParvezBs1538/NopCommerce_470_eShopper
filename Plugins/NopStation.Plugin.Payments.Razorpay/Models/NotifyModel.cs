﻿using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.Razorpay.Models
{
    public class NotifyModel
    {
        [JsonProperty("sms")]
        public bool Sms { get; set; }

        [JsonProperty("email")]
        public bool Email { get; set; }
    }
}
