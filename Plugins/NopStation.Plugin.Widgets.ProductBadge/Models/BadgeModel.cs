using Nop.Web.Framework.Models;
using NopStation.Plugin.Widgets.ProductBadge.Domains;

namespace NopStation.Plugin.Widgets.ProductBadge.Models;

public record BadgeModel : BaseNopEntityModel
{
    public ContentType ContentType { get; set; }

    public ShapeType ShapeType { get; set; }

    public string Text { get; set; }

    public string BackgroundColor { get; set; }

    public string PictureUrl { get; set; }

    public string FontColor { get; set; }

    public Size Size { get; set; }

    public PositionType PositionType { get; set; }

    public string CssClass { get; set; }
}