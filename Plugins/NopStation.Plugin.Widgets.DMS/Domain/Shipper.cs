using System;
using Nop.Core;

namespace NopStation.Plugin.Widgets.DMS.Domain
{
    public class Shipper : BaseEntity
    {
        public int CustomerId { get; set; }

        public bool Active { get; set; }

        public bool IsAvailable { get; set; }

        public DateTime CreatedOnUtc { get; set; }
    }
}
