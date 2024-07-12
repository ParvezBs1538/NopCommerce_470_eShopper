using System.Collections.Generic;
using Newtonsoft.Json;

namespace NopStation.Plugin.CRM.Zoho.Services.Zoho.Request
{
    public class ContactsRequest : BaseZohoParentType
    {
        [JsonProperty("data")]
        public List<ContactRequest> Data = new List<ContactRequest>();
    }

    public class ContactRequest : BaseZohoType
    {
        [JsonProperty("Email")]
        public string Email { get; set; }

        [JsonProperty("Vendor_Name")]
        public VendorRequest Vendor { get; set; }

        [JsonProperty("First_Name")]
        public string FirstName { get; set; }

        [JsonProperty("Phone")]
        public string Phone { get; set; }

        [JsonProperty("Date_of_Birth")]
        public string DateOfBirth { get; set; }

        [JsonProperty("Last_Name")]
        public string LastName { get; set; }
    }
}
