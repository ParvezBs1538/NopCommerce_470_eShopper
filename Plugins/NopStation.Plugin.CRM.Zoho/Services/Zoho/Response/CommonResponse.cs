using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace NopStation.Plugin.CRM.Zoho.Services.Zoho.Response
{
    public class ModifiedBy
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class CreatedBy
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }

    public class Details
    {
        [JsonProperty("api_name")]
        public string ApiName { get; set; }

        [JsonProperty("parent_api_name")]
        public string ParentApiName { get; set; }

        [JsonProperty("Modified_By")]
        public ModifiedBy UpdatedBy { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("Created_By")]
        public CreatedBy CreatedBy { get; set; }

        [JsonProperty("Modified_Time")]
        public DateTime? UpdatedOn { get; set; }

        [JsonProperty("Created_Time")]
        public DateTime? CreatedOn { get; set; }
    }

    public class Datum
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("details")]
        public Details Details { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }

    public class CommonResponse
    {
        [JsonProperty("data")]
        public List<Datum> Data { get; set; }
    }
}
