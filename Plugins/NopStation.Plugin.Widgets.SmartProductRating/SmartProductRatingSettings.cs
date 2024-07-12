using Nop.Core.Configuration;

namespace NopStation.Plugin.Widgets.SmartProductRating
{
    public class SmartProductRatingSettings : ISettings
    {
        public bool EnablePlugin { get; set; }

        public int NumberOfReviewsInProductDetailsPage { get; set; }

        public string ProductDetailsPageWidgetZone { get; set; }
    }
}
