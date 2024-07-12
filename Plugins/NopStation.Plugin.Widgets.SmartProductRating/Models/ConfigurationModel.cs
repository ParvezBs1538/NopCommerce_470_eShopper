using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.SmartProductRating.Models
{
    public class ConfigurationModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Admin.NopStation.SmartProductRating.Configuration.Fields.EnablePlugin")]
        public bool EnablePlugin { get; set; }
        public bool EnablePlugin_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.SmartProductRating.Configuration.Fields.NumberOfReviewsInProductDetailsPage")]
        public int NumberOfReviewsInProductDetailsPage { get; set; }
        public bool NumberOfReviewsInProductDetailsPage_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.SmartProductRating.Configuration.Fields.ProductDetailsPageWidgetZone")]
        public string ProductDetailsPageWidgetZone { get; set; }
        public bool ProductDetailsPageWidgetZone_OverrideForStore { get; set; }
    }
}
