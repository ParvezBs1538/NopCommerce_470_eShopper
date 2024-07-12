using Nop.Core.Configuration;

namespace NopStation.Plugin.Widgets.GoogleAnalytics
{
    public class NopstationGoogleAnalyticsSettings : ISettings
    {
        public string GoogleId { get; set; }
        public string ApiSecret { get; set; }
        public string TrackingScript { get; set; }
        public bool EnableEcommerce { get; set; }
        public bool UseJsToSendEcommerceInfo { get; set; }
        public bool IncludingTax { get; set; }
        public string WidgetZone { get; set; }
        public bool IncludeCustomerId { get; set; }
        public bool SaveLog { get; set; }
    }
}