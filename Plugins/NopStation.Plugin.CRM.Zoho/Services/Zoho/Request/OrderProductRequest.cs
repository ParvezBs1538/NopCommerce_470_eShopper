using System.Collections.Generic;
using Newtonsoft.Json;

namespace NopStation.Plugin.CRM.Zoho.Services.Zoho.Request
{
    public class OrderProductRequest
    {
        [JsonProperty("product")]
        public ProductRequest Product { get; set; }

        [JsonProperty("quantity")]
        public int Quantity { get; set; }

        [JsonProperty("Discount")]
        public decimal Discount { get; set; }

        [JsonProperty("total_after_discount")]
        public decimal TotalAfterDiscount { get; set; }

        [JsonProperty("net_total")]
        public decimal NetTotal { get; set; }

        [JsonProperty("Tax")]
        public decimal Tax { get; set; }

        [JsonProperty("list_price")]
        public decimal ListPrice { get; set; }

        [JsonProperty("unit_price")]
        public decimal UnitPrice { get; set; }

        [JsonProperty("quantity_in_stock")]
        public int QuantityInStock { get; set; }

        [JsonProperty("total")]
        public decimal Total { get; set; }

        [JsonProperty("product_description")]
        public string ProductDescription { get; set; }

        [JsonProperty("line_tax")]
        public List<OrderTaxRequest> Taxes { get; set; }
    }
}
