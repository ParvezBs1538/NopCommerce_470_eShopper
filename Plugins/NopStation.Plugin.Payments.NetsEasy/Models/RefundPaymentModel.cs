using System.Collections.Generic;
using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.NetsEasy.Models
{
    public class RefundPaymentModel
    {
        [JsonProperty("amount")]
        public int Amount { get; set; }

        [JsonProperty("orderItems")]
        public IList<OrderItem> OrderItems { get; set; }

        public class OrderItem
        {
            [JsonProperty("reference")]
            public string Reference { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("quantity")]
            public int Quantity { get; set; }

            [JsonProperty("unit")]
            public string Unit { get; set; }

            [JsonProperty("unitPrice")]
            public int UnitPrice { get; set; }

            [JsonProperty("taxRate")]
            public int TaxRate { get; set; }

            [JsonProperty("taxAmount")]
            public int TaxAmount { get; set; }

            [JsonProperty("grossTotalAmount")]
            public int GrossTotalAmount { get; set; }

            [JsonProperty("netTotalAmount")]
            public int NetTotalAmount { get; set; }
        }
    }

    public class RefundPaymentResponseModel
    {
        [JsonProperty("refundId")]
        public string RefundId { get; set; }
    }
}
