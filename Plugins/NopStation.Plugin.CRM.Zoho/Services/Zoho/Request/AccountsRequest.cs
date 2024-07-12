using System.Collections.Generic;
using Newtonsoft.Json;

namespace NopStation.Plugin.CRM.Zoho.Services.Zoho.Request
{
    public class AccountsRequest : BaseZohoParentType
    {
        [JsonProperty("data")]
        public List<AccountRequest> Data = new List<AccountRequest>();
    }

    public class AccountRequest : BaseZohoType
    {
        [JsonProperty("Website")]
        public string Website { get; set; }

        [JsonProperty("Phone")]
        public string Phone { get; set; }

        [JsonProperty("Account_Name")]
        public string AccountName { get; set; }

        [JsonProperty("$approved")]
        public bool Approved { get; set; }
    }
}
