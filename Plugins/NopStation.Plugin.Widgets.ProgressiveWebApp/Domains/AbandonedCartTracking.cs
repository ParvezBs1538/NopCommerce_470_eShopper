using System;
using Nop.Core;

namespace NopStation.Plugin.Widgets.ProgressiveWebApp.Domains
{
    public class AbandonedCartTracking : BaseEntity
    {
        public int CustomerId { get; set; }

        public DateTime LastModifiedOnUtc { get; set; }

        public bool IsQueued { get; set; }
    }
}
