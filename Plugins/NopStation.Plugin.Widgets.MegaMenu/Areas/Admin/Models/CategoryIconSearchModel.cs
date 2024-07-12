using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.MegaMenu.Areas.Admin.Models;

public record CategoryIconSearchModel : BaseSearchModel
{
    public CategoryIconSearchModel()
    {
        AvailableStores = new List<SelectListItem>();
    }

    [NopResourceDisplayName("Admin.NopStation.MegaMenu.CategoryIcons.List.SearchCategoryName")]
    public string SearchCategoryName { get; set; }

    [NopResourceDisplayName("Admin.NopStation.MegaMenu.CategoryIcons.List.SearchStore")]
    public int SearchStoreId { get; set; }
    public IList<SelectListItem> AvailableStores { get; set; }

    public bool HideStoresList { get; set; }
}
