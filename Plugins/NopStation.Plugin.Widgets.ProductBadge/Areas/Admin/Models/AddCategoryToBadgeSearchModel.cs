using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.ProductBadge.Areas.Admin.Models;

public partial record AddCategoryToBadgeSearchModel : BaseSearchModel
{
    #region Properties

    [NopResourceDisplayName("Admin.Catalog.Categories.List.SearchCategoryName")]
    public string SearchCategoryName { get; set; }

    #endregion
}