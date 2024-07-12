using System.Collections.Generic;
using Newtonsoft.Json;

namespace NopStation.Plugin.CRM.Zoho.Services.Zoho.Request
{
    public class SalesOrdersRequest : BaseZohoParentType
    {
        [JsonProperty("data")]
        public List<SalesOrderRequest> Data = new List<SalesOrderRequest>();
    }

    public class SalesOrderRequest : BaseZohoType
    {
        public SalesOrderRequest()
        {
            Products = new List<OrderProductRequest>();
        }

        [JsonProperty("$currency_symbol")]
        public string CurrencySymbol { get; set; }

        [JsonProperty("Customer_No")]
        public string CustomerNo { get; set; }

        [JsonProperty("Tax")]
        public decimal Tax { get; set; }

        [JsonProperty("Exchange_Rate")]
        public decimal ExchangeRate { get; set; }

        [JsonProperty("Currency")]
        public string Currency { get; set; }

        [JsonProperty("Billing_Country")]
        public string BillingCountry { get; set; }

        [JsonProperty("Carrier")]
        public string Carrier { get; set; }

        [JsonProperty("Status")]
        public string Status { get; set; }

        [JsonProperty("Grand_Total")]
        public decimal GrandTotal { get; set; }

        [JsonProperty("Billing_Street")]
        public string BillingStreet { get; set; }

        [JsonProperty("Billing_Code")]
        public string BillingCode { get; set; }

        [JsonProperty("Product_Details")]
        public List<OrderProductRequest> Products { get; set; }

        [JsonProperty("Shipping_City")]
        public string ShippingCity { get; set; }

        [JsonProperty("Shipping_Country")]
        public string ShippingCountry { get; set; }

        [JsonProperty("Shipping_Code")]
        public string ShippingCode { get; set; }

        [JsonProperty("Billing_City")]
        public string BillingCity { get; set; }

        [JsonProperty("Shipping_Street")]
        public string ShippingStreet { get; set; }

        [JsonProperty("Discount")]
        public decimal Discount { get; set; }

        [JsonProperty("Shipping_State")]
        public string ShippingState { get; set; }

        [JsonProperty("Account_Name")]
        public AccountRequest Account { get; set; }

        [JsonProperty("Terms_and_Conditions")]
        public string TermsAndConditions { get; set; }

        [JsonProperty("Sub_Total")]
        public decimal SubTotal { get; set; }

        [JsonProperty("Subject")]
        public string Subject { get; set; }

        [JsonProperty("Contact_Name")]
        public ContactRequest Contact { get; set; }

        [JsonProperty("Billing_State")]
        public string BillingState { get; set; }
    }
}
