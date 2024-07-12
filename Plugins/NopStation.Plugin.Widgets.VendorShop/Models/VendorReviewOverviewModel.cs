using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.VendorShop.Models
{
    public record VendorReviewOverviewModel : BaseNopModel
    {
        public string BasedOnTotalReview { get; set; }
        public string OverallRating { get; set; }
    }
}
