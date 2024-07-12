using System;
using System.Collections.Generic;
using System.Text;

namespace NopStation.Plugin.Payments.BlueSnapHosted.Models
{
    public class WebhookSubscription
    {
        public Guid OrderGuidId { set; get; }
        public string SubscriptionId { set; get; }
        public string Type { set; get; }
    }
}
