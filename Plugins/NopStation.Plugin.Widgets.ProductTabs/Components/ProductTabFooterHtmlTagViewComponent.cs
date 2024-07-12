using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Components;

namespace NopStation.Plugin.Widgets.ProductTabs.Components
{
    public class ProductTabFooterHtmlTagViewComponent : NopStationViewComponent
    {
        public ProductTabFooterHtmlTagViewComponent()
        {
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            return View();
        }
    }
}
