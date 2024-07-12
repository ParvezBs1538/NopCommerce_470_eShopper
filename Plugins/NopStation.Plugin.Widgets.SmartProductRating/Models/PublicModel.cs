using System.Collections.Generic;
using Nop.Web.Models.Catalog;

namespace NopStation.Plugin.Widgets.SmartProductRating.Models
{
    public class PublicModel
    {
        public PublicModel()
        {
            ProductReviews = new List<ProductReviewModel>();
        }

        public int ProductId { get; set; }

        public int RatingSum { get; set; }

        public int TotalReviews { get; set; }

        public decimal ReviewPercentage { get; set; }

        public int FiveStarReviews { get; set; }

        public int FourStarReviews { get; set; }

        public int ThreeStarReviews { get; set; }

        public int TwoStarReviews { get; set; }

        public int OneStarReviews { get; set; }

        public decimal FiveStarPercentage { get; set; }

        public decimal FourStarPercentage { get; set; }

        public decimal ThreeStarPercentage { get; set; }

        public decimal TwoStarPercentage { get; set; }

        public decimal OneStarPercentage { get; set; }

        public IList<ProductReviewModel> ProductReviews { get; set; }
        public string ProductName { get; internal set; }
        public string ProductSeName { get; internal set; }
    }
}
