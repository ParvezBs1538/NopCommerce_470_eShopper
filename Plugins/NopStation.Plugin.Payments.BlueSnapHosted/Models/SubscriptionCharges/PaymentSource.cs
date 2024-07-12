using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.BlueSnapHosted.Models.SubscriptionCharges
{
    public class PaymentSource
    {
        [JsonProperty("creditCardInfo")]
        public CreditCardInfo CreditCardInfo { get; set; }
    }
}
