using System;
using Nop.Core;

namespace NopStation.Plugin.Misc.IpFilter.Domain
{
    public class CountryBlockRule : BaseEntity
    {
        public int CountryId { get; set; }

        public string Comment { get; set; }

        public DateTime CreatedOnUtc { get; set; }
    }
}