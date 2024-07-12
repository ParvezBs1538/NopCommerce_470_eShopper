using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.ProductBadge.Areas.Admin.Models;

public partial record AddManufacturerToBadgeSearchModel : BaseSearchModel
{
    #region Properties

    [NopResourceDisplayName("Admin.Catalog.Manufacturers.List.SearchManufacturerName")]
    public string SearchManufacturerName { get; set; }

    #endregion
}