using System;
using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.Afterpay.Models
{
    public class Consumer
    {
        [JsonProperty("phoneNumber")]
        public string PhoneNumber { get; set; }

        [JsonProperty("givenNames")]
        public string GivenNames { get; set; }

        [JsonProperty("surname")]
        public string SurName { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }
    }

    public class AfterpayAddress
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("line1")]
        public string Line1 { get; set; }

        [JsonProperty("line2")]
        public string Line2 { get; set; }

        [JsonProperty("area1")]
        public string Area1 { get; set; }

        [JsonProperty("area2")]
        public string Area2 { get; set; }

        [JsonProperty("region")]
        public string Region { get; set; }

        [JsonProperty("postcode")]
        public string PostCode { get; set; }

        [JsonProperty("countryCode")]
        public string CountryCode { get; set; }

        [JsonProperty("phoneNumber")]
        public string PhoneNumber { get; set; }
    }

    public class Merchant
    {
        [JsonProperty("redirectConfirmUrl")]
        public string RedirectConfirmUrl { get; set; }

        [JsonProperty("redirectCancelUrl")]
        public string RedirectCancelUrl { get; set; }
    }

    public class Courier
    {
        [JsonProperty("shippedAt")]
        public DateTime ShippedAt { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("tracking")]
        public string Tracking { get; set; }

        [JsonProperty("priority")]
        public string Priority { get; set; }
    }

    public class Item
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("sku")]
        public string Sku { get; set; }

        [JsonProperty("quantity")]
        public int Quantity { get; set; }

        [JsonProperty("price")]
        public PaymentAfterpayModel Price { get; set; }

        [JsonProperty("pageUrl")]
        public string PageUrl { get; set; }

        [JsonProperty("imageUrl")]
        public string ImageUrl { get; set; }
    }

    public class Discount
    {
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("amount")]
        public PaymentAfterpayModel Amount { get; set; }
    }

    public class Event
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("created")]
        public DateTime Created { get; set; }

        [JsonProperty("expires")]
        public DateTime? Expires { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("amount")]
        public PaymentAfterpayModel Amount { get; set; }

        [JsonProperty("paymentEventMerchantReference")]
        public object PaymentEventMerchantReference { get; set; }
    }
}
