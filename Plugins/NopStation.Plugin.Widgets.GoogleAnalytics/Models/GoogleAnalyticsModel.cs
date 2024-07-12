using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.GoogleAnalytics.Models
{
    public record GoogleAnalyticsModel : BaseNopEntityModel
    {
        public string Scripts { get; set; }
    }
}
