using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.GoogleAnalytics.Models
{
    public record ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Widgets.GoogleAnalytics.GoogleId")]
        public string GoogleId { get; set; }
        public bool GoogleId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Widgets.GoogleAnalytics.EnableEcommerce")]
        public bool EnableEcommerce { get; set; }
        public bool EnableEcommerce_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Widgets.GoogleAnalytics.UseJsToSendEcommerceInfo")]
        public bool UseJsToSendEcommerceInfo { get; set; }
        public bool UseJsToSendEcommerceInfo_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Widgets.GoogleAnalytics.TrackingScript")]
        public string TrackingScript { get; set; }
        public bool TrackingScript_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Widgets.GoogleAnalytics.SaveLog")]
        public bool SaveLog { get; set; }
        public bool SaveLog_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Widgets.GoogleAnalytics.IncludingTax")]
        public bool IncludingTax { get; set; }
        public bool IncludingTax_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Widgets.GoogleAnalytics.IncludeCustomerId")]
        public bool IncludeCustomerId { get; set; }
        public bool IncludeCustomerId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Widgets.GoogleAnalytics.ApiSecret")]
        public string ApiSecret { get; set; }
        public bool ApiSecret_OverrideForStore { get; set; }
    }
}