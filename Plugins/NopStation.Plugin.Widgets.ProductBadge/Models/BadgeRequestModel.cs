using System.Collections.Generic;

namespace NopStation.Plugin.Widgets.ProductBadge.Models;

public class BadgeRequestModel
{
    public BadgeRequestModel()
    {
        DetailsProductIds = new List<int>();
        OverviewProductIds = new List<int>();
    }

    public IList<int> DetailsProductIds { get; set; }
    public IList<int> OverviewProductIds { get; set; }
}