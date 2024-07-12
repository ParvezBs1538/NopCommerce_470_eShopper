using System;
using Nop.Core;
using Nop.Core.Domain.Common;

namespace NopStation.Plugin.Widgets.PushNop.Domains
{
    public class SmartGroup : BaseEntity, ISoftDeletedEntity
    {
        public string Name { get; set; }

        public bool Deleted { get; set; }

        public DateTime CreatedOnUtc { get; set; }

        public DateTime UpdatedOnUtc { get; set; }
    }
}
