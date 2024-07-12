using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.NetsEasy.Models.Response
{
    public class RetrievePaymentModel
    {
        [JsonProperty("payment")]
        public Payment Payment { get; set; }
    }
    public class RetrieveSubscriptionChargeModel
    {
        [JsonProperty("page")]
        public List<Page> Pages { get; set; }

        [JsonProperty("more")]
        public bool More { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }
    public class Page
    {
        [JsonProperty("subscriptionId")]
        public string SubscriptionId { get; set; }

        [JsonProperty("paymentId")]
        public string PaymentId { get; set; }

        [JsonProperty("chargeId")]
        public string ChargeId { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }
    public class Summary
    {
        [JsonProperty("reservedAmount")]
        public int ReservedAmount { get; set; }

        [JsonProperty("chargedAmount")]
        public int ChargedAmount { get; set; }

        [JsonProperty("refundedAmount")]
        public int RefundedAmount { get; set; }

        [JsonProperty("cancelledAmount")]
        public int CancelledAmount { get; set; }
    }

    public class ShippingAddress
    {
        [JsonProperty("addressLine1")]
        public string AddressLine1 { get; set; }

        [JsonProperty("addressLine2")]
        public string AddressLine2 { get; set; }

        [JsonProperty("receiverLine")]
        public string ReceiverLine { get; set; }

        [JsonProperty("postalCode")]
        public string PostalCode { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }
    }

    public class PhoneNumber
    {
        [JsonProperty("prefix")]
        public string Prefix { get; set; }

        [JsonProperty("number")]
        public string Number { get; set; }
    }

    public class ContactDetails
    {
        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("phoneNumber")]
        public PhoneNumber PhoneNumber { get; set; }
    }

    public class Company
    {
        [JsonProperty("merchantReference")]
        public string MerchantReference { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("registrationNumber")]
        public string RegistrationNumber { get; set; }

        [JsonProperty("contactDetails")]
        public ContactDetails ContactDetails { get; set; }
    }

    public class PrivatePerson
    {
        [JsonProperty("merchantReference")]
        public string MerchantReference { get; set; }

        [JsonProperty("dateOfBirth")]
        public DateTime DateOfBirth { get; set; }

        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("phoneNumber")]
        public PhoneNumber PhoneNumber { get; set; }
    }

    public class BillingAddress
    {
        [JsonProperty("addressLine1")]
        public string AddressLine1 { get; set; }

        [JsonProperty("addressLine2")]
        public string AddressLine2 { get; set; }

        [JsonProperty("receiverLine")]
        public string ReceiverLine { get; set; }

        [JsonProperty("postalCode")]
        public string PostalCode { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }
    }

    public class Consumer
    {
        [JsonProperty("shippingAddress")]
        public ShippingAddress ShippingAddress { get; set; }

        [JsonProperty("company")]
        public Company Company { get; set; }

        [JsonProperty("privatePerson")]
        public PrivatePerson PrivatePerson { get; set; }

        [JsonProperty("billingAddress")]
        public BillingAddress BillingAddress { get; set; }
    }

    public class InvoiceDetails
    {
        [JsonProperty("invoiceNumber")]
        public string InvoiceNumber { get; set; }

        [JsonProperty("ocr")]
        public string Ocr { get; set; }

        [JsonProperty("pdfLink")]
        public string PdfLink { get; set; }

        [JsonProperty("dueDate")]
        public DateTime DueDate { get; set; }
    }

    public class CardDetails
    {
        [JsonProperty("maskedPan")]
        public string MaskedPan { get; set; }

        [JsonProperty("expiryDate")]
        public string ExpiryDate { get; set; }
    }

    public class PaymentDetails
    {
        [JsonProperty("paymentType")]
        public string PaymentType { get; set; }

        [JsonProperty("paymentMethod")]
        public string PaymentMethod { get; set; }

        [JsonProperty("invoiceDetails")]
        public InvoiceDetails InvoiceDetails { get; set; }

        [JsonProperty("cardDetails")]
        public CardDetails CardDetails { get; set; }
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

    public class Checkout
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("cancelUrl")]
        public string CancelUrl { get; set; }
    }

    public class OrderItem
    {
        [JsonProperty("reference")]
        public string Reference { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("quantity")]
        public decimal Quantity { get; set; }

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

    public class Refund
    {
        [JsonProperty("refundId")]
        public string RefundId { get; set; }

        [JsonProperty("amount")]
        public int Amount { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("lastUpdated")]
        public DateTime LastUpdated { get; set; }

        [JsonProperty("orderItems")]
        public List<OrderItem> OrderItems { get; set; }
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

    public class Subscription
    {
        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class Payment
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
        public Checkout Checkout { get; set; }

        [JsonProperty("created")]
        public DateTime Created { get; set; }

        [JsonProperty("refunds")]
        public List<Refund> Refunds { get; set; }

        [JsonProperty("charges")]
        public List<Charge> Charges { get; set; }

        [JsonProperty("terminated")]
        public DateTime Terminated { get; set; }

        [JsonProperty("subscription")]
        public Subscription Subscription { get; set; }
    }

}
