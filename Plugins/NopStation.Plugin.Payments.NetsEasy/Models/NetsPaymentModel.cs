using Newtonsoft.Json;
using NopStation.Plugin.Payments.NetsEasy.Models.Response;
using System;
using System.Collections.Generic;
namespace NopStation.Plugin.Payments.NetsEasy.Models
{
    public class VerifyPaymentModel
    {
        [JsonProperty("payment")]
        public NetsPaymentModel Payment { get; set; }
    }
    
    public class NetsPaymentModel
    {
        [JsonProperty("paymentId")]
        public string PaymentId { get; set; }

        [JsonProperty("summary")]
        public Summary Summary { get; set; }

        [JsonProperty("consumer")]
        public Consumer Consumer { get; set; }

        [JsonProperty("paymentDetails")]
        public PaymentDetails PaymentDetails { get; set; }

        [JsonProperty("orderDetails")]
        public OrderDetails OrderDetails { get; set; }

        [JsonProperty("checkout")]
        public object Checkout { get; set; }

        [JsonProperty("created")]
        public DateTime Created { get; set; }

        [JsonProperty("charges")]
        public List<Charge> Charges { get; set; }

        [JsonProperty("subscription")]
        public Subscription Subscription { get; set; }
    }

    public class Summary
    {
        [JsonProperty("reservedAmount")]
        public int ReservedAmount { get; set; }

        [JsonProperty("chargedAmount")]
        public int ChargedAmount { get; set; }
    }

    public class PaymentDetails
    {
        [JsonProperty("paymentType")]
        public string PaymentType { get; set; }

        [JsonProperty("paymentMethod")]
        public string PaymentMethod { get; set; }

        [JsonProperty("invoiceDetails")]
        public object InvoiceDetails { get; set; }

        [JsonProperty("cardDetails")]
        public CardDetails CardDetails { get; set; }
    }

    public class CardDetails
    {
        [JsonProperty("maskedPan")]
        public string MaskedPan { get; set; }

        [JsonProperty("expiryDate")]
        public string ExpiryDate { get; set; }
    }

    public class OrderDetails
    {
        [JsonProperty("amount")]
        public int Amount { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("reference")]
        public string Reference { get; set; }
    }

    public class Charge
    {
        [JsonProperty("chargeId")]
        public string ChargeId { get; set; }

        [JsonProperty("amount")]
        public int Amount { get; set; }

        [JsonProperty("created")]
        public DateTime Created { get; set; }

        [JsonProperty("orderItems")]
        public List<OrderItem> OrderItems { get; set; }
    }

    public class OrderItem
    {
        [JsonProperty("reference")]
        public string Reference { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("quantity")]
        public double Quantity { get; set; }

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