using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.SmartMegaMenu.Areas.Admin.Models;

public partial record AddCategoryToMegaMenuSearchModel : BaseSearchModel
{
    #region Properties

    public int MegaMenuId { get; set; }

    [NopResourceDisplayName("Admin.Catalog.Categories.List.SearchCategoryName")]
    public string SearchCategoryName { get; set; }

    #endregion
}