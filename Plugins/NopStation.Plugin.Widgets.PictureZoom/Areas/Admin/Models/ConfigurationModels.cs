using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.PictureZoom.Areas.Admin.Models;

public partial record ConfigurationModel : BaseNopModel, ISettingsModel
{
    [NopResourceDisplayName("Admin.NopStation.PictureZoom.Configuration.Fields.EnablePictureZoom")]
    public bool EnablePictureZoom { get; set; }
    public bool EnablePictureZoom_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.PictureZoom.Configuration.Fields.ZoomWidth")]
    public double ZoomWidth { get; set; }
    public bool ZoomWidth_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.PictureZoom.Configuration.Fields.ZoomHeight")]
    public double ZoomHeight { get; set; }
    public bool ZoomHeight_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.PictureZoom.Configuration.Fields.LtrPositionTypeId")]
    public int LtrPositionTypeId { get; set; }
    public bool LtrPositionTypeId_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.PictureZoom.Configuration.Fields.RtlPositionTypeId")]
    public int RtlPositionTypeId { get; set; }
    public bool RtlPositionTypeId_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.PictureZoom.Configuration.Fields.Tint")]
    public bool Tint { get; set; }
    public bool Tint_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.PictureZoom.Configuration.Fields.TintOpacity")]
    public double TintOpacity { get; set; }
    public bool TintOpacity_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.PictureZoom.Configuration.Fields.LensOpacity")]
    public double LensOpacity { get; set; }
    public bool LensOpacity_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.PictureZoom.Configuration.Fields.SoftFocus")]
    public bool SoftFocus { get; set; }
    public bool SoftFocus_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.PictureZoom.Configuration.Fields.SmoothMove")]
    public int SmoothMove { get; set; }
    public bool SmoothMove_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.PictureZoom.Configuration.Fields.ShowTitle")]
    public bool ShowTitle { get; set; }
    public bool ShowTitle_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.PictureZoom.Configuration.Fields.TitleOpacity")]
    public double TitleOpacity { get; set; }
    public bool TitleOpacity_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.PictureZoom.Configuration.Fields.AdjustX")]
    public int AdjustX { get; set; }
    public bool AdjustX_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.PictureZoom.Configuration.Fields.AdjustY")]
    public int AdjustY { get; set; }
    public bool AdjustY_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.PictureZoom.Configuration.Fields.ImageSize")]
    public int ImageSize { get; set; }
    public bool ImageSize_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.PictureZoom.Configuration.Fields.FullSizeImage")]
    public int FullSizeImage { get; set; }
    public bool FullSizeImage_OverrideForStore { get; set; }

    public int ActiveStoreScopeConfiguration { get; set; }
}