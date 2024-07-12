using Newtonsoft.Json;

namespace NopStation.Plugin.CRM.Zoho.Services.Zoho
{
    public abstract class BaseZohoParentType
    {
        [JsonProperty("duplicate_check_fields")]
        public string[] DuplicateCheckFields = { "NopEntityId" };
    }
}