using System.Collections.Generic;
using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.NetsEasy.Models
{
    public class CreatePaymentModel
    {
        public CreatePaymentModel()
        {
            Order = new Order();
            Checkout = new Checkout();
            Notifications = new Notifications();
            PaymentMethods = new List<PaymentMethod>();
            //Subscriptions = new Subscriptions();
        }

        [JsonProperty("order")]
        public Order Order { get; set; }

        [JsonProperty("checkout")]
        public Checkout Checkout { get; set; }

        [JsonProperty("merchantNumber")]
        public string MerchantNumber { get; set; }

        [JsonProperty("notifications")]
        public Notifications Notifications { get; set; }

        [JsonProperty("subscription")]
        public Subscription Subscription { get; set; }

        [JsonProperty("paymentMethods")]
        public List<PaymentMethod> PaymentMethods { get; set; }
    }

    public class Subscription
    {
        public Subscription()
        {
            Order= new Order();
        }
        [JsonProperty("subscriptionId")]

        public string SubscriptionId { get; set; }

        [JsonProperty("endDate")]
        public string EndDate { get; set; }

        [JsonProperty("interval")]
        public int Interval { get; set; }

        [JsonProperty("order")]
        public Order Order { get; set; }
    }

    public class Item
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

    public class Order
    {
        public Order()
        {
            Items = new List<Item>();
        }

        [JsonProperty("items")]
        public List<Item> Items { get; set; }

        [JsonProperty("amount")]
        public int Amount { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("reference")]
        public string Reference { get; set; }
    }

    public class Country
    {
        [JsonProperty("countryCode")]
        public string CountryCode { get; set; }
    }

    public class Shipping
    {
        public Shipping()
        {
            Countries = new List<Country>();
        }

        [JsonProperty("countries")]
        public List<Country> Countries { get; set; }

        [JsonProperty("merchantHandlesShippingCost")]
        public bool MerchantHandlesShippingCost { get; set; }

        [JsonProperty("enableBillingAddress")]
        public bool EnableBillingAddress { get; set; }
    }

    public class ConsumerType
    {
        public ConsumerType()
        {
            SupportedTypes = new List<string>();
        }

        [JsonProperty("supportedTypes")]
        public List<string> SupportedTypes { get; set; }

        [JsonProperty("default")]
        public string Default { get; set; }
    }

    public class Consumer
    {
        public Consumer()
        {
            ShippingAddress = new ShippingAddress();
            PhoneNumber = new PhoneNumber();
            PrivatePerson = new PrivatePerson();
        }

        [JsonProperty("reference")]
        public string Reference { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("shippingAddress")]
        public ShippingAddress ShippingAddress { get; set; }

        [JsonProperty("phoneNumber")]
        public PhoneNumber PhoneNumber { get; set; }

        [JsonProperty("privatePerson")]
        public PrivatePerson PrivatePerson { get; set; }

        //[JsonProperty("company")]
        //public Company Company { get; set; }
    }

    public class ShippingAddress
    {
        [JsonProperty("addressLine1")]
        public string AddressLine1 { get; set; }

        [JsonProperty("addressLine2")]
        public string AddressLine2 { get; set; }

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

    public class PrivatePerson
    {
        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }
    }

    public class Company
    {
        public Company()
        {
            Contact = new Contact();
        }
        public string Name { get; set; }

        public Contact Contact { get; set; }
    }

    public class Contact
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class Checkout
    {
        public Checkout()
        {
            ShippingCountries = new List<Country>();
            Shipping = new Shipping();
            ConsumerType = new ConsumerType();
            Appearance = new Appearance();
            Consumer = new Consumer();
        }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("integrationType")]
        public string IntegrationType { get; set; }

        [JsonProperty("returnUrl")]
        public string ReturnUrl { get; set; }

        [JsonProperty("cancelUrl")]
        public string CancelUrl { get; set; }

        [JsonProperty("consumer")]
        public Consumer Consumer { get; set; }

        [JsonProperty("termsUrl")]
        public string TermsUrl { get; set; }

        [JsonProperty("merchantTermsUrl")]
        public string MerchantTermsUrl { get; set; }

        [JsonProperty("shippingCountries")]
        public List<Country> ShippingCountries { get; set; }

        [JsonProperty("shipping")]
        public Shipping Shipping { get; set; }

        [JsonProperty("consumerType")]
        public ConsumerType ConsumerType { get; set; }

        [JsonProperty("charge")]
        public bool Charge { get; set; }

        [JsonProperty("publicDevice")]
        public bool PublicDevice { get; set; }

        [JsonProperty("merchantHandlesConsumerData")]
        public bool MerchantHandlesConsumerData { get; set; }

        public Appearance Appearance { get; set; }

    }

    public class Appearance
    {
        public Appearance()
        {
            DisplayOptions = new DisplayOptions();
            TextOptions = new TextOptions();
        }
        public DisplayOptions DisplayOptions { get; set; }

        public TextOptions TextOptions { get; set; }
    }

    public class DisplayOptions
    {
        public bool ShowMerchantName { get; set; }

        public bool ShowOrderSummary { get; set; }
    }

    public class TextOptions
    {
        public string CompletePaymentButtonText { get; set; }
    }

    public class Webhook
    {
        [JsonProperty("eventName")]
        public string EventName { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("authorization")]
        public string Authorization { get; set; }

        [JsonProperty("headers")]
        public string Headers { get; set; }
    }

    public class Notifications
    {
        public Notifications()
        {
            Webhooks = new List<Webhook>();
        }

        [JsonProperty("webhooks")]
        public List<Webhook> Webhooks { get; set; }
    }

    public class Fee
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

    public class PaymentMethod
    {
        public PaymentMethod()
        {
            Fee = new Fee();
        }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("fee")]
        public Fee Fee { get; set; }
    }

    public class CreatePaymentResponse
    {
        [JsonProperty("paymentId")]
        public string PaymentId { get; set; }
    }
    public class ChargeSubscriptions
    {
        public ChargeSubscriptions()
        {
            Subscriptions = new List<Subscription>();
        }
        [JsonProperty("subscriptions")]
        public List<Subscription> Subscriptions { get; set; }

        [JsonProperty("externalBulkChargeId")]
        public string ChargeId { get; set; }
    }
}
