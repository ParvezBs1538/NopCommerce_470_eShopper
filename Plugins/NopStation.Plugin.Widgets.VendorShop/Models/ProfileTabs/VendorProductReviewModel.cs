using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Infrastructure;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Common;

namespace NopStation.Plugin.Widgets.VendorShop.Models.ProfileTabs
{
    public partial record VendorProductReviewModel : BaseNopModel
    {
        public VendorProductReviewModel()
        {
            AdditionalProductReviewList = new List<ProductReviewReviewTypeMappingModel>();
        }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductSeName { get; set; }
        public string Title { get; set; }
        public string ReviewText { get; set; }
        public int Rating { get; set; }
        public string WrittenOnStr { get; set; }
        public string ApprovalStatus { get; set; }
        public IList<ProductReviewReviewTypeMappingModel> AdditionalProductReviewList { get; set; }
    }

    public partial record VendorProductReviewsModel : BaseNopModel
    {
        public VendorProductReviewsModel()
        {
            ProductReviews = new List<VendorProductReviewModel>();
        }

        public IList<VendorProductReviewModel> ProductReviews { get; set; }
        public PagerModel PagerModel { get; set; }

        #region Nested class

        public partial record VendorProductReviewsRouteValues : IRouteValues
        {
            public int PageNumber { get; set; }
        }

        #endregion
    }
}
