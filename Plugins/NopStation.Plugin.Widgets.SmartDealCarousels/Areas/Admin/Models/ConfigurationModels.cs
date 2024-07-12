using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Widgets.SmartDealCarousels.Areas.Admin.Models;

public partial record ConfigurationModel : BaseNopModel, ISettingsModel
{
    [NopResourceDisplayName("Admin.NopStation.SmartDealCarousels.Configuration.Fields.EnableCarousel")]
    public bool EnableCarousel { get; set; }
    public bool EnableCarousel_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartDealCarousels.Configuration.Fields.EnableAjaxLoad")]
    public bool EnableAjaxLoad { get; set; }
    public bool EnableAjaxLoad_OverrideForStore { get; set; }

    [NopResourceDisplayName("Admin.NopStation.SmartDealCarousels.Configuration.Fields.CarouselPictureSize")]
    public int CarouselPictureSize { get; set; }
    public bool CarouselPictureSize_OverrideForStore { get; set; }

    public int ActiveStoreScopeConfiguration { get; set; }
}