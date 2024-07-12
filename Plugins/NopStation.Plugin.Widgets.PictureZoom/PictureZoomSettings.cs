using Nop.Core.Configuration;

namespace NopStation.Plugin.Widgets.PictureZoom;

public class PictureZoomSettings : ISettings
{
    public bool EnablePictureZoom { get; set; }

    public double ZoomWidth { get; set; } = 1;

    public double ZoomHeight { get; set; } = 1;

    public int LtrPositionTypeId { get; set; } = (int)PictureZoomPosition.Right;

    public int RtlPositionTypeId { get; set; } = (int)PictureZoomPosition.Right;

    public bool Tint { get; set; } = false;

    public double TintOpacity { get; set; } = .5;

    public double LensOpacity { get; set; } = .5;

    public bool SoftFocus { get; set; } = false;

    public int SmoothMove { get; set; } = 3;

    public bool ShowTitle { get; set; } = true;

    public double TitleOpacity { get; set; } = .5;

    public int AdjustX { get; set; } = 0;

    public int AdjustY { get; set; } = 0;

    public int ImageSize { get; set; } = 500;

    public int FullSizeImage { get; set; } = 1000;
}