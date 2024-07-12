using Nop.Core.Configuration;

namespace NopStation.Plugin.Widgets.OrderRatings
{
    public class OrderRatingSettings : ISettings
    {
        public bool OpenOrderRatingPopupInHomepage { get; set; }

        public bool ShowOrderRatedDateInDetailsPage { get; set; }

        public string OrderDetailsPageWidgetZone { get; set; }
    }
}
