using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.StoreLocator.Models;
using Nop.Web.Framework.Infrastructure;

namespace NopStation.Plugin.Widgets.StoreLocator.Components
{
    public class StoreLocatorTopMenuViewComponent : NopStationViewComponent
    {
        private readonly StoreLocatorSettings _storeLocatorSettings;

        public StoreLocatorTopMenuViewComponent(StoreLocatorSettings storeLocatorSettings)
        {
            _storeLocatorSettings = storeLocatorSettings;
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            if (!_storeLocatorSettings.EnablePlugin)
                return Content("");

            if (!_storeLocatorSettings.IncludeInTopMenu)
                return Content("");

            if(widgetZone == PublicWidgetZones.MobHeaderMenuAfter && _storeLocatorSettings.HideInMobileView)
                return Content("");

            var model = new TopMenuModel() 
            { 
                MobileHeaderMenu = widgetZone == PublicWidgetZones.MobHeaderMenuAfter 
            };

            return View(model);
        }
    }
}
