using System.Collections.Generic;
using Newtonsoft.Json;

namespace NopStation.Plugin.Payments.Affirm.Models
{
    public class MerchantModel
    {
        [JsonProperty("user_confirmation_url")]
        public string UserConfirmationUrl { get; set; }

        [JsonProperty("user_cancel_url")]
        public string UserCancelUrl { get; set; }

        [JsonProperty("user_confirmation_url_action")]
        public string UserConfirmationUrlAction { get; set; }

        [JsonProperty("merchant")]
        public string Merchant { get; set; }
    }

    public class NameModel
    {
        [JsonProperty("first")]
        public string First { get; set; }

        [JsonProperty("last")]
        public string Last { get; set; }
    }

    public class AddressModel
    {
        [JsonProperty("line1")]
        public string Line1 { get; set; }

        [JsonProperty("line2")]
        public string Line2 { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("zipcode")]
        public string Zipcode { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }
    }

    public class AddressDetailsModel
    {
        public AddressDetailsModel()
        {
            Name = new NameModel();
            Address = new AddressModel();
        }

        [JsonProperty("name")]
        public NameModel Name { get; set; }

        [JsonProperty("address")]
        public AddressModel Address { get; set; }

        [JsonProperty("phone_number")]
        public string PhoneNumber { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }
    }

    public class ItemModel
    {
        public ItemModel()
        {
            //Categories = new List<List<string>>();
        }

        [JsonProperty("display_name")]
        public string DisplayName { get; set; }

        [JsonProperty("sku")]
        public string Sku { get; set; }

        [JsonProperty("unit_price")]
        public int UnitPrice { get; set; }

        [JsonProperty("qty")]
        public int Qty { get; set; }

        //[JsonProperty("item_image_url")]
        //public string ItemImageUrl { get; set; }

        //[JsonProperty("item_url")]
        //public string ItemUrl { get; set; }

        //[JsonProperty("categories")]
        //public List<List<string>> Categories { get; set; }
    }

    public class Return5Model
    {
        [JsonProperty("discount_amount")]
        public int DiscountAmount { get; set; }

        [JsonProperty("discount_display_name")]
        public string DiscountDisplayName { get; set; }
    }

    public class Presday10Model
    {
        [JsonProperty("discount_amount")]
        public int DiscountAmount { get; set; }

        [JsonProperty("discount_display_name")]
        public string DiscountDisplayName { get; set; }
    }

    public class DiscountsModel
    {
        public DiscountsModel()
        {
            Return5 = new Return5Model();
            Presday10 = new Presday10Model();
        }

        [JsonProperty("RETURN5")]
        public Return5Model Return5 { get; set; }

        [JsonProperty("PRESDAY10")]
        public Presday10Model Presday10 { get; set; }
    }

    public class MetadataModel
    {
        [JsonProperty("shipping_type")]
        public string ShippingType { get; set; }

        [JsonProperty("mode")]
        public string Mode { get; set; }
    }

    public class JsonRootModel
    {
        public JsonRootModel()
        {
            Merchant = new MerchantModel();
            Shipping = new AddressDetailsModel();
            Billing = new AddressDetailsModel();
            Items = new List<ItemModel>();
            //Discounts = new DiscountsModel();
            Metadata = new MetadataModel();
        }

        [JsonProperty("merchant")]
        public MerchantModel Merchant { get; set; }

        [JsonProperty("shipping")]
        public AddressDetailsModel Shipping { get; set; }

        [JsonProperty("billing")]
        public AddressDetailsModel Billing { get; set; }

        [JsonProperty("items")]
        public List<ItemModel> Items { get; set; }

        //[JsonProperty("discounts")]
        //public DiscountsModel Discounts { get; set; }

        [JsonProperty("metadata")]
        public MetadataModel Metadata { get; set; }

        [JsonProperty("order_id")]
        public string OrderId { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        //[JsonProperty("financing_program")]
        //public string FinancingProgram { get; set; }

        [JsonProperty("shipping_amount")]
        public decimal ShippingAmount { get; set; }

        [JsonProperty("tax_amount")]
        public decimal TaxAmount { get; set; }

        [JsonProperty("total")]
        public int Total { get; set; }
    }
}
