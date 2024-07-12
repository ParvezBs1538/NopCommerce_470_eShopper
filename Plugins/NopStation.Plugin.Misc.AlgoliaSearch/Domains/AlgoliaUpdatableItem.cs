using System;
using Nop.Core;

namespace NopStation.Plugin.Misc.AlgoliaSearch.Domains
{
    public partial class AlgoliaUpdatableItem : BaseEntity
    {
        public int EntityId { get; set; }

        public string EntityName { get; set; }

        public int LastUpdatedBy { get; set; }

        public DateTime UpdatedOnUtc { get; set; }
    }
}
