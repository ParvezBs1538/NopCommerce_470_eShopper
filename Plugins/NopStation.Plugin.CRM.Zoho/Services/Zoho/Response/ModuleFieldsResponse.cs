using System.Collections.Generic;
using Newtonsoft.Json;

namespace NopStation.Plugin.CRM.Zoho.Services.Zoho.Response
{
    public class ModuleFieldsResponse
    {
        [JsonProperty("fields")]
        public List<ModuleFieldResponse> Fields { get; set; }

        public string Error { get; set; }
    }

    public class ModuleFieldResponse
    {
        [JsonProperty("system_mandatory")]
        public bool SystemMandatory { get; set; }

        [JsonProperty("json_type")]
        public string JsonType { get; set; }

        [JsonProperty("field_label")]
        public string FieldLabel { get; set; }

        [JsonProperty("display_label")]
        public string DisplayLabel { get; set; }

        [JsonProperty("lookup")]
        public LookupResponse Lookup { get; set; }

        [JsonProperty("api_name")]
        public string ApiName { get; set; }

        [JsonProperty("data_type")]
        public string DataType { get; set; }

        [JsonProperty("custom_field")]
        public bool CustomField { get; set; }
    }

    public class LookupResponse
    {
        [JsonProperty("display_label")]
        public string DisplayLabel { get; set; }

        [JsonProperty("api_name")]
        public string ApiName { get; set; }

        [JsonProperty("module")]
        public string Module { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
    }
}
