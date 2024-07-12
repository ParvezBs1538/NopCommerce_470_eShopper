using System.Collections.Generic;
using Newtonsoft.Json;

namespace NopStation.Plugin.CRM.Zoho.Services.Zoho.Response
{
    public class ModulesResponse
    {
        [JsonProperty("modules")]
        public List<ModuleResponse> Modules { get; set; }

        public string Error { get; set; }
    }

    public class ModuleResponse
    {
        [JsonProperty("creatable")]
        public bool Creatable { get; set; }

        [JsonProperty("plural_label")]
        public string PluralLabel { get; set; }

        [JsonProperty("api_supported")]
        public bool ApiSupported { get; set; }

        [JsonProperty("api_name")]
        public string ApiName { get; set; }

        [JsonProperty("module_name")]
        public string ModuleName { get; set; }
    }
}
