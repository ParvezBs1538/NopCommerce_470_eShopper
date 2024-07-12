using System;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.CrawlerManager.Models
{
    public record CrawlerModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("NopStation.Plugin.Widgets.CrawlerManager.Fields.CrawlerInfo")]
        public string CrawlerInfo { get; set; }

        [NopResourceDisplayName("NopStation.Plugin.Widgets.CrawlerManager.Fields.IPAddress")]
        public string IPAddress { get; set; }

        [NopResourceDisplayName("NopStation.Plugin.Widgets.CrawlerManager.Fields.Location")]
        public string Location { get; set; }

        [NopResourceDisplayName("NopStation.Plugin.Widgets.CrawlerManager.Fields.AddedOnUtc")]
        public DateTime AddedOnUtc { get; set; }

        [NopResourceDisplayName("NopStation.Plugin.Widgets.CrawlerManager.Fields.AddedBy")]
        public string AddedBy { get; set; }
    }
}