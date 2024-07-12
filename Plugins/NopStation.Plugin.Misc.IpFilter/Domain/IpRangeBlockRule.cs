using System;
using Nop.Core;

namespace NopStation.Plugin.Misc.IpFilter.Domain
{
    public class IpRangeBlockRule : BaseEntity
    {
        public string FromIpAddress { get; set; }

        public string ToIpAddress { get; set; }

        public string Comment { get; set; }

        public DateTime CreatedOnUtc { get; set; }
    }
}
