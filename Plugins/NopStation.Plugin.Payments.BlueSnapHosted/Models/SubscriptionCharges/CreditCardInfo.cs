using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.BlueSnapHosted.Models.SubscriptionCharges
{
    public class CreditCardInfo
    {
        [JsonProperty("billingContactInfo")]
        public BillingContactInfo BillingContactInfo { get; set; }
        [JsonProperty("creditCard")]
        public CreditCard CreditCard { get; set; }
    }
}
