using System;
using Nop.Core;

namespace NopStation.Plugin.Widgets.CrawlerManager.Domain
{
    public class Crawler : BaseEntity
    {
        public string CrawlerInfo { get; set; }
        public string IPAddress { get; set; }
        public string Location { get; set; }
        public DateTime AddedOnUtc { get; set; }
        public string AddedBy { get; set; }
    }
}
