using Nop.Core;
using Nop.Core.Domain.Localization;

namespace NopStation.Plugin.Widgets.SmartSliders.Domains;

public class SmartSliderItem : BaseEntity, ILocalizedEntity
{
    public int SliderId { get; set; }

    public string Title { get; set; }

    public string Description { get; set; }

    public string RedirectUrl { get; set; }

    public string ButtonText { get; set; }

    public int DisplayOrder { get; set; }

    public bool ShowCaption { get; set; }

    public int ContentTypeId { get; set; }

    public int DesktopPictureId { get; set; }

    public int MobilePictureId { get; set; }

    public int SliderVideoId { get; set; }

    public string EmbeddedLink { get; set; }

    public string Text { get; set; }

    public int LanguageId { get; set; }

    public ContentType ContentType
    {
        get => (ContentType)ContentTypeId;
        set => ContentTypeId = (int)value;
    }
}
