using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.SmartMegaMenu.Areas.Admin.Models;

public partial record AddVendorToMegaMenuSearchModel : BaseSearchModel
{
    #region Properties

    public int MegaMenuId { get; set; }

    [NopResourceDisplayName("Admin.Vendors.List.SearchName")]
    public string SearchName { get; set; }

    [NopResourceDisplayName("Admin.Vendors.List.SearchEmail")]
    public string SearchEmail { get; set; }

    #endregion
}