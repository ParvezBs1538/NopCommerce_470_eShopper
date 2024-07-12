using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.Affirm.Models
{
    public class EventResponseModel
    {
        [JsonProperty("created")]
        public DateTime Created { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("transaction_id")]
        public string TransactionId { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public class SweaterA92123
    {
        [JsonProperty("sku")]
        public string Sku { get; set; }

        [JsonProperty("display_name")]
        public string DisplayName { get; set; }

        [JsonProperty("qty")]
        public int Qty { get; set; }

        [JsonProperty("item_type")]
        public string ItemType { get; set; }

        [JsonProperty("item_image_url")]
        public string ItemImageUrl { get; set; }

        [JsonProperty("item_url")]
        public string ItemUrl { get; set; }

        [JsonProperty("unit_price")]
        public int UnitPrice { get; set; }
    }

    public class ItemsResponseModel
    {
        [JsonProperty("sweater-a92123")]
        public SweaterA92123 SweaterA92123 { get; set; }
    }

    public class NameResponseModel
    {
        [JsonProperty("full")]
        public string Full { get; set; }
    }

    public class AddressResponseModel
    {
        [JsonProperty("line1")]
        public string Line1 { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("zipcode")]
        public string Zipcode { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }
    }

    public class ShippingResponseModel
    {
        public ShippingResponseModel()
        {
            Name = new NameResponseModel();
            Address = new AddressResponseModel();
        }

        [JsonProperty("name")]
        public NameResponseModel Name { get; set; }

        [JsonProperty("address")]
        public AddressResponseModel Address { get; set; }

        [JsonProperty("phone_number")]
        public string PhoneNumber { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }
    }

    public class Return5ResponseModel
    {
        [JsonProperty("discount_amount")]
        public int DiscountAmount { get; set; }

        [JsonProperty("discount_display_name")]
        public string DiscountDisplayName { get; set; }
    }

    public class Presday10ResponseModel
    {
        [JsonProperty("discount_amount")]
        public int DiscountAmount { get; set; }

        [JsonProperty("discount_display_name")]
        public string DiscountDisplayName { get; set; }
    }

    public class DiscountsResponseModel
    {
        public DiscountsResponseModel()
        {
            Return5 = new Return5ResponseModel();
            Presday10 = new Presday10ResponseModel();
        }

        [JsonProperty("RETURN5")]
        public Return5ResponseModel Return5 { get; set; }

        [JsonProperty("PRESDAY10")]
        public Presday10ResponseModel Presday10 { get; set; }
    }

    public class DetailsResponseModel
    {
        public DetailsResponseModel()
        {
            Items = new ItemsResponseModel();
            Shipping = new ShippingResponseModel();
            Discounts = new DiscountsResponseModel();
        }

        [JsonProperty("items")]
        public ItemsResponseModel Items { get; set; }

        [JsonProperty("order_id")]
        public string OrderId { get; set; }

        [JsonProperty("shipping_amount")]
        public int ShippingAmount { get; set; }

        [JsonProperty("tax_amount")]
        public int TaxAmount { get; set; }

        [JsonProperty("shipping")]
        public ShippingResponseModel Shipping { get; set; }

        [JsonProperty("discounts")]
        public DiscountsResponseModel Discounts { get; set; }
    }

    public class ChargeJsonModel
    {
        public ChargeJsonModel()
        {
            Events = new List<EventResponseModel>();
            Details = new DetailsResponseModel();
        }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("created")]
        public DateTime Created { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("amount")]
        public int Amount { get; set; }

        [JsonProperty("auth_hold")]
        public int AuthHold { get; set; }

        [JsonProperty("payable")]
        public int Payable { get; set; }

        [JsonProperty("void")]
        public bool Void { get; set; }

        [JsonProperty("expires")]
        public DateTime Expires { get; set; }

        [JsonProperty("order_id")]
        public string OrderId { get; set; }

        [JsonProperty("events")]
        public List<EventResponseModel> Events { get; set; }

        [JsonProperty("details")]
        public DetailsResponseModel Details { get; set; }
    }
}
