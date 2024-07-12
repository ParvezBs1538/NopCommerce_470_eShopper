using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.SmartMegaMenu.Areas.Admin.Models;

public partial record AddManufacturerToMegaMenuSearchModel : BaseSearchModel
{
    #region Properties

    public int MegaMenuId { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Manufacturers.List.SearchManufacturerName")]
    public string SearchManufacturerName { get; set; }

    #endregion
}