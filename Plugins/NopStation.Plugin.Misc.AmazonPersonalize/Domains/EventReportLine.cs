using System;

namespace NopStation.Plugin.Misc.AmazonPersonalize.Domains
{
    public partial class EventReportLine
    {
        public int UserId { get; set; }
        public int ItemId { get; set; }
        
        public DateTime CreatedOnUtc { get; set; }
    }
}