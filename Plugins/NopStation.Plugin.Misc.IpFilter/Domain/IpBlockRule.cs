using System;
using Nop.Core;

namespace NopStation.Plugin.Misc.IpFilter.Domain
{
    public class IpBlockRule : BaseEntity
    {
        public string IpAddress { get; set; }

        public string Location { get; set; }

        public string Comment { get; set; }

        public DateTime CreatedOnUtc { get; set; }

        public bool IsAllowed { get; set; }
    }
}
