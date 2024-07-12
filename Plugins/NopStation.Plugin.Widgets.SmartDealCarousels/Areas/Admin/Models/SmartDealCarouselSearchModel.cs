using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.SmartDealCarousels.Areas.Admin.Models;

public record SmartDealCarouselSearchModel : BaseSearchModel
{
    public SmartDealCarouselSearchModel()
    {
        AvailableProductSources = new List<SelectListItem>();
        AvailableWidgetZones = new List<SelectListItem>();
        AvailableStores = new List<SelectListItem>();
        AvailableActiveOptions = new List<SelectListItem>();
        AvailableCarouselTypes = new List<SelectListItem>();
    }

    [NopResourceDisplayName("Admin.NopStation.SmartDealCarousels.Carousels.List.SearchKeyword")]
    public string SearchKeyword { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartDealCarousels.Carousels.List.SearchWidgetZone")]
    public string SearchWidgetZone { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartDealCarousels.Carousels.List.SearchProductSource")]
    public int SearchProductSourceId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartDealCarousels.Carousels.List.SearchStore")]
    public int SearchStoreId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartDealCarousels.Carousels.List.SearchActive")]
    public int SearchActiveId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartDealCarousels.Carousels.List.SearchCarouselType")]
    public int SearchCarouselTypeId { get; set; }

    public IList<SelectListItem> AvailableWidgetZones { get; set; }
    public IList<SelectListItem> AvailableProductSources { get; set; }
    public IList<SelectListItem> AvailableStores { get; set; }
    public IList<SelectListItem> AvailableActiveOptions { get; set; }
    public IList<SelectListItem> AvailableCarouselTypes { get; set; }
}