using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.ProductBadge.Areas.Admin.Models;

public record BadgeSearchModel : BaseSearchModel
{
    public BadgeSearchModel()
    {
        AvailableStores = new List<SelectListItem>();
        AvailableActiveOptions = new List<SelectListItem>();
        AvailableBadgeTypes = new List<SelectListItem>();
    }

    [NopResourceDisplayName("Admin.NopStation.ProductBadge.Badges.List.SearchKeyword")]
    public string SearchKeyword { get; set; }

    [NopResourceDisplayName("Admin.NopStation.ProductBadge.Badges.List.SearchStore")]
    public int SearchStoreId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.ProductBadge.Badges.List.SearchActive")]
    public int SearchActiveId { get; set; }

    [NopResourceDisplayName("Admin.NopStation.ProductBadge.Badges.List.SearchBadgeType")]
    public int SearchBadgeTypeId { get; set; }

    public IList<SelectListItem> AvailableStores { get; set; }
    public IList<SelectListItem> AvailableActiveOptions { get; set; }
    public IList<SelectListItem> AvailableBadgeTypes { get; set; }
}