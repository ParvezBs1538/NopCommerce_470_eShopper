using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.SmartSliders.Areas.Admin.Models;

public record SmartSliderSearchModel : BaseSearchModel
{
    public SmartSliderSearchModel()
    {
        AvailableStores = new List<SelectListItem>();
        AvailableActiveOptions = new List<SelectListItem>();
        AvailableWidgetZones = new List<SelectListItem>();
    }

    [NopResourceDisplayName("Admin.NopStation.SmartSliders.Sliders.List.SearchKeyword")]
    public string SearchKeyword { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartSliders.Sliders.List.SearchWidgetZone")]
    public string SearchWidgetZone { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartSliders.Sliders.List.SearchStore")]
    public int SearchStoreId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartSliders.Sliders.List.SearchActive")]
    public int SearchActiveId { get; set; }

    public IList<SelectListItem> AvailableStores { get; set; }
    public IList<SelectListItem> AvailableActiveOptions { get; set; }
    public IList<SelectListItem> AvailableWidgetZones { get; set; }
}
