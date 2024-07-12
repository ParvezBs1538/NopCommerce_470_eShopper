using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.UI.Paging;

namespace NopStation.Plugin.Widgets.VendorShop.Models.ProfileTabs
{
    public partial record VendorProfileModel : BasePageableModel
    {
        #region Properties

        public bool UseAjaxLoading { get; set; }

        public int VendorId { get; set; }
        //public string ShopDescription { get; set; }
        public string WarningMessage { get; set; }

        public string NoResultMessage { get; set; }
        public VendorReviewOverviewModel ReviewOverview { get; set; }
        public bool HasReview { get; set; }

        public IList<SelectListItem> AvailableSortOptions { get; set; }

        public IList<SelectListItem> AvailableFilterOptions { get; set; }

        public int? OrderBy { get; set; }

        public int? FilterBy { get; set; }
        public IList<VendorProductReviewModel> Reviews { get; set; }

        #endregion

        #region Ctor

        public VendorProfileModel()
        {

            AvailableSortOptions = new List<SelectListItem>();
            Reviews = new List<VendorProductReviewModel>();
            AvailableFilterOptions = new List<SelectListItem>();
        }

        #endregion
    }
}
