using Newtonsoft.Json;

namespace NopStation.Plugin.CRM.Zoho.Services.Zoho
{
    public abstract class BaseZohoType
    {
        [JsonProperty("NopEntityId")]
        public string NopEntityId { get; set; }

        [JsonProperty("id")]
        public string ZohoId { get; set; }
    }
}