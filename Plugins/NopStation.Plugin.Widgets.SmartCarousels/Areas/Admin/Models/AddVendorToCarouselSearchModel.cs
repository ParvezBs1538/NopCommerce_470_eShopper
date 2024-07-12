using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.SmartCarousels.Areas.Admin.Models;

public partial record AddVendorToCarouselSearchModel : BaseSearchModel
{
    #region Properties

    [NopResourceDisplayName("Admin.Vendors.List.SearchName")]
    public string SearchName { get; set; }

    [NopResourceDisplayName("Admin.Vendors.List.SearchEmail")]
    public string SearchEmail { get; set; }

    #endregion
}