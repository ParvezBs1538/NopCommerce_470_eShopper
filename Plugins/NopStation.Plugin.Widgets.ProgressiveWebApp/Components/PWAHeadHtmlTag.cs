using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Components;

namespace NopStation.Plugin.Widgets.ProgressiveWebApp.Components
{
    public class PWAHeadHtmlTagViewComponent : NopStationViewComponent
    {
        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            return View();
        }
    }
}