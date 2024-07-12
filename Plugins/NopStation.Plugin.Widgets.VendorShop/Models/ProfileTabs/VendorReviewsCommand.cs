using Nop.Web.Framework.UI.Paging;

namespace NopStation.Plugin.Widgets.VendorShop.Models.ProfileTabs
{
    public partial record VendorReviewsCommand : BasePageableModel
    {
        #region Properties
        public int? OrderBy { get; set; }

        public int? FilterBy { get; set; }

        #endregion
    }
}
