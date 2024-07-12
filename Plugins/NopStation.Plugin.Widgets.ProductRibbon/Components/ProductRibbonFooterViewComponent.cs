using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Components;

namespace NopStation.Plugin.Widgets.ProductRibbon.Components
{
    public class ProductRibbonFooterViewComponent : NopStationViewComponent
    {
        public IViewComponentResult Invoke(string widgetZone, object additionalData = null)
        {
            return View();
        }
    }
}
