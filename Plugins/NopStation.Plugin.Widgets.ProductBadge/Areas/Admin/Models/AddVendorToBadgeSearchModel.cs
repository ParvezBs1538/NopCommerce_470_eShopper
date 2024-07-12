using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.ProductBadge.Areas.Admin.Models;

public partial record AddVendorToBadgeSearchModel : BaseSearchModel
{
    #region Properties

    [NopResourceDisplayName("Admin.Vendors.List.SearchName")]
    public string SearchName { get; set; }

    [NopResourceDisplayName("Admin.Vendors.List.SearchEmail")]
    public string SearchEmail { get; set; }

    #endregion
}