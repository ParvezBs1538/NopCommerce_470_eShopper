using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.OrderRatings.Areas.Admin.Models
{
    public partial record ConfigurationModel : BaseNopModel, ISettingsModel
    {
        [NopResourceDisplayName("Admin.NopStation.OrderRatings.Configuration.Fields.OpenOrderRatingPopupInHomepage")]
        public bool OpenOrderRatingPopupInHomepage { get; set; }
        public bool OpenOrderRatingPopupInHomepage_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.OrderRatings.Configuration.Fields.ShowOrderRatedDateInDetailsPage")]
        public bool ShowOrderRatedDateInDetailsPage { get; set; }
        public bool ShowOrderRatedDateInDetailsPage_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.OrderRatings.Configuration.Fields.OrderDetailsPageWidgetZone")]
        public string OrderDetailsPageWidgetZone { get; set; }
        public bool OrderDetailsPageWidgetZone_OverrideForStore { get; set; }

        public int ActiveStoreScopeConfiguration { get; set; }
    }
}