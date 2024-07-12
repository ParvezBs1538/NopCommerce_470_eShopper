using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Widgets.ProductBadge.Models;

public record BadgeAjaxModel : BaseNopModel
{
    public string WidgetZone { get; set; }

    public bool DetailsPage { get; set; }

    public int ProductId { get; set; }
}