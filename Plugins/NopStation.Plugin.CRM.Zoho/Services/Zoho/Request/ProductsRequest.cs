using System.Collections.Generic;
using Newtonsoft.Json;

namespace NopStation.Plugin.CRM.Zoho.Services.Zoho.Request
{
    public class ProductsRequest : BaseZohoParentType
    {
        [JsonProperty("data")]
        public List<ProductRequest> Data = new List<ProductRequest>();
    }

    public class ProductRequest : BaseZohoType
    {
        [JsonProperty("Product_Category")]
        public string ProductCategory { get; set; }

        [JsonProperty("Description")]
        public string Description { get; set; }

        [JsonProperty("Vendor_Name")]
        public VendorRequest Vendor { get; set; }

        [JsonProperty("Tax")]
        public List<string> Taxes { get; set; }

        [JsonProperty("Sales_Start_Date")]
        public string SalesStartDate { get; set; }

        [JsonProperty("Product_Active")]
        public bool ProductActive { get; set; }

        [JsonProperty("Record_Image")]
        public string RecordImage { get; set; }

        [JsonProperty("Product_Code")]
        public string ProductCode { get; set; }

        [JsonProperty("Manufacturer")]
        public string Manufacturer { get; set; }

        [JsonProperty("Product_Name")]
        public string ProductName { get; set; }

        [JsonProperty("Qty_in_Stock")]
        public int QtyInStock { get; set; }

        [JsonProperty("Tag")]
        public List<string> Tags { get; set; }

        [JsonProperty("Sales_End_Date")]
        public string SalesEndDate { get; set; }

        [JsonProperty("Unit_Price")]
        public decimal UnitPrice { get; set; }

        [JsonProperty("Taxable")]
        public bool Taxable { get; set; }
    }
}
