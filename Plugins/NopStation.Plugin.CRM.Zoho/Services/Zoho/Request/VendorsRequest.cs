using System.Collections.Generic;
using Newtonsoft.Json;

namespace NopStation.Plugin.CRM.Zoho.Services.Zoho.Request
{
    public class VendorsRequest : BaseZohoParentType
    {
        [JsonProperty("data")]
        public List<VendorRequest> Data = new List<VendorRequest>();
    }

    public class VendorRequest : BaseZohoType
    {
        [JsonProperty("Email")]
        public string Email { get; set; }

        [JsonProperty("Category")]
        public string Category { get; set; }

        [JsonProperty("Description")]
        public string Description { get; set; }

        [JsonProperty("$currency_symbol")]
        public string CurrencySymbol { get; set; }

        [JsonProperty("Vendor_Name")]
        public string VendorName { get; set; }

        [JsonProperty("Website")]
        public string Website { get; set; }

        [JsonProperty("State")]
        public string State { get; set; }

        [JsonProperty("Phone")]
        public string Phone { get; set; }

        [JsonProperty("Street")]
        public string Street { get; set; }

        [JsonProperty("Zip_Code")]
        public string ZipCode { get; set; }

        [JsonProperty("$approved")]
        public bool Approved { get; set; }

        [JsonProperty("City")]
        public string City { get; set; }

        [JsonProperty("GL_Account")]
        public string GLAccount { get; set; }

        [JsonProperty("Country")]
        public string Country { get; set; }
    }
}
