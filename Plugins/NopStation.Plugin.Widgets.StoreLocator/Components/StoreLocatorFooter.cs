using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.StoreLocator.Helpers;
using NopStation.Plugin.Widgets.StoreLocator.Models;

namespace NopStation.Plugin.Widgets.StoreLocator.Components
{
    public class StoreLocatorFooterViewComponent : NopStationViewComponent
    {
        private readonly StoreLocatorSettings _storeLocatorSettings;

        public StoreLocatorFooterViewComponent(StoreLocatorSettings storeLocatorSettings)
        {
            _storeLocatorSettings = storeLocatorSettings;
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            if (!_storeLocatorSettings.EnablePlugin)
                return Content("");

            if (!_storeLocatorSettings.IncludeInFooterColumn && !_storeLocatorSettings.SortPickupPointsByDistance)
                return Content("");

            var model = new FooterModel()
            {
                IncludeInFooterColumn = _storeLocatorSettings.IncludeInFooterColumn,
                FooterColumnSelector = _storeLocatorSettings.FooterColumnSelector,
                AskCustomerLocation = _storeLocatorSettings.SortPickupPointsByDistance && 
                    StoreLocatorHelper.GetGeoLocationAsync() == null
            };

            return View(model);
        }
    }
}
