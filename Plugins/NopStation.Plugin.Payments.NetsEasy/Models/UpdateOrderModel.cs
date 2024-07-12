using System.Collections.Generic;
using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.NetsEasy.Models
{
    public class UpdateOrderModel
    {
        [JsonProperty("amount")]
        public int Amount { get; set; }

        [JsonProperty("items")]
        public List<Item> Items { get; set; }

        [JsonProperty("shipping")]
        public Shipping UpdateOrderShipping { get; set; }

        [JsonProperty("paymentMethods")]
        public List<PaymentMethod> PaymentMethods { get; set; }

        public class Shipping
        {
            [JsonProperty("costSpecified")]
            public bool CostSpecified { get; set; }
        }
    }
}
