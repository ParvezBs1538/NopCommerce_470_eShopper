using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.SmartMegaMenu.Areas.Admin.Models;

public record MegaMenuSearchModel : BaseSearchModel
{
    public MegaMenuSearchModel()
    {
        AvailableStores = new List<SelectListItem>();
        AvailableActiveOptions = new List<SelectListItem>();
        AvailableWidgetZones = new List<SelectListItem>();
    }

    [NopResourceDisplayName("Admin.NopStation.SmartMegaMenu.MegaMenus.List.SearchKeyword")]
    public string SearchKeyword { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartMegaMenu.MegaMenus.List.SearchStore")]
    public int SearchStoreId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartMegaMenu.MegaMenus.List.SearchActive")]
    public int SearchActiveId { get; set; }

    public IList<SelectListItem> AvailableStores { get; set; }
    public IList<SelectListItem> AvailableWidgetZones { get; set; }
    public IList<SelectListItem> AvailableActiveOptions { get; set; }
}
