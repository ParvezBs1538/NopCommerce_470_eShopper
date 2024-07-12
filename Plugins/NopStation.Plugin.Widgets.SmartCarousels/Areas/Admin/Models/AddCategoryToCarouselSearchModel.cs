using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.SmartCarousels.Areas.Admin.Models;

public partial record AddCategoryToCarouselSearchModel : BaseSearchModel
{
    #region Properties

    [NopResourceDisplayName("Admin.Catalog.Categories.List.SearchCategoryName")]
    public string SearchCategoryName { get; set; }

    #endregion
}