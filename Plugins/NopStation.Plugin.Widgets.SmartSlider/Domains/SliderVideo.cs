using Nop.Core;

namespace NopStation.Plugin.Widgets.SmartSliders.Domains;

public class SliderVideo : BaseEntity
{
    public string MimeType { get; set; }

    public string SeoFilename { get; set; }

    public string AltAttribute { get; set; }

    public string TitleAttribute { get; set; }

    public bool IsNew { get; set; }

    public int ThumbnailPictureId { get; set; }
}
