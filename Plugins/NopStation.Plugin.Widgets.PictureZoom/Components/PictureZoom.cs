using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Components;

namespace NopStation.Plugin.Widgets.PictureZoom.Components;

public class PictureZoomViewComponent : NopStationViewComponent
{
    public IViewComponentResult Invoke(string widgetZone, object additionalData)
    {
        return View();
    }
}
