using Nop.Core;

namespace NopStation.Plugin.CRM.Zoho.Domain
{
    public partial class UpdatableItem : BaseEntity
    {
        public int EntityId { get; set; }

        public int EntityTypeId { get; set; }
    }
}
