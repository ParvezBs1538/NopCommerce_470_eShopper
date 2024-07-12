using System.Collections.Generic;
using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.NetsEasy.Models
{
    public class ChargePaymentModel
    {
        [JsonProperty("amount")]
        public int Amount { get; set; }

        [JsonProperty("orderItems")]
        public IList<OrderItem> OrderItems { get; set; }

        [JsonProperty("shipping")]
        public Shipping ChargeShipping { get; set; }

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

        public class Shipping
        {
            [JsonProperty("trackingNumber")]
            public int TrackingNumber { get; set; }

            [JsonProperty("provider")]
            public int Provider { get; set; }
        }
    }

    public class ChargePaymentResponseModel
    {
        [JsonProperty("chargeId")]
        public string ChargeId { get; set; }

        [JsonProperty("invoice")]
        public Invoice ChargeInvoice { get; set; }

        public class Invoice
        {
            [JsonProperty("invoiceNumber")]
            public string InvoiceNumber { get; set; }
        }
    }
    public class ChargeSubscriptionsResponseModel
    {
        [JsonProperty("bulkId")]
        public string BulkId { get; set; }
    }
}
