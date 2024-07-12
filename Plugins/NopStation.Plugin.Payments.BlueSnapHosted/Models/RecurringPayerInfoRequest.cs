using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.BlueSnapHosted.Models
{
    public class RecurringPayerInfoRequest
    {
        public RecurringPayerInfoRequest()
        {
            PayerInfo = new PayerInfo();
            PaymentSource = new PaymentSource();
        }

        [JsonProperty("planId")]
        public int PlanId { set; get; }
        public PayerInfo PayerInfo { set; get; }
        public PaymentSource PaymentSource { set; get; }
    }
    
    public class PayerInfo
    {
        [JsonProperty("zip")]
        public string Zip { set; get; }
        [JsonProperty("firstName")]
        public string FirstName { set; get; }
        [JsonProperty("lastName")]
        public string LastName { set; get; }
    }

    public class PaymentSource
    {
        [JsonProperty("pfToken")]
        public string PfToken { set; get; }
    }
}
