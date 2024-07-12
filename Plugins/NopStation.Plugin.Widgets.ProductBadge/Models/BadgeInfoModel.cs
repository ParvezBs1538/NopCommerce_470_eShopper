using System.Collections.Generic;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.ProductBadge.Models;

public record BadgeInfoModel : BaseNopEntityModel
{
    public BadgeInfoModel()
    {
        Badges = new List<BadgeModel>();
    }

    public int ProductId { get; set; }

    public bool DetailsPage { get; set; }

    public IList<BadgeModel> Badges { get; set; }
}