using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.Popups.Areas.Admin.Models;

public record PopupSearchModel : BaseSearchModel
{
    public PopupSearchModel()
    {
        AvailableStores = new List<SelectListItem>();
        AvailableActiveOptions = new List<SelectListItem>();
    }

    [NopResourceDisplayName("Admin.NopStation.Popups.Popups.List.SearchStore")]
    public int SearchStoreId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.Popups.Popups.List.SearchActive")]
    public int SearchActiveId { get; set; }

    public IList<SelectListItem> AvailableStores { get; set; }
    public IList<SelectListItem> AvailableActiveOptions { get; set; }
}
