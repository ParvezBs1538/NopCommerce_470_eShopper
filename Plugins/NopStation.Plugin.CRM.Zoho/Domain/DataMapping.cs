using Nop.Core;

namespace NopStation.Plugin.CRM.Zoho.Domain
{
    public class DataMapping : BaseEntity
    {
        public int EntityId { get; set; }

        public int EntityTypeId { get; set; }

        public string ZohoId { get; set; }
    }
}
