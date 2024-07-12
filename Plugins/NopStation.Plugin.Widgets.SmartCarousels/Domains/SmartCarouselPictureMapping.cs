using Nop.Core;
using Nop.Core.Domain.Localization;

namespace NopStation.Plugin.Widgets.SmartCarousels.Domains;

public partial class SmartCarouselPictureMapping : BaseEntity, ILocalizedEntity
{
    public int PictureId { get; set; }

    public string Label { get; set; }

    public string RedirectUrl { get; set; }

    public int CarouselId { get; set; }

    public int DisplayOrder { get; set; }
}
