using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.Helpdesk.Models;

namespace NopStation.Plugin.Widgets.Helpdesk.Components
{
    public class HelpdeskNavigationViewComponent : NopStationViewComponent
    {
        private readonly HelpdeskSettings _helpdeskSettings;

        public HelpdeskNavigationViewComponent(HelpdeskSettings helpdeskSettings)
        {
            _helpdeskSettings = helpdeskSettings;
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            var model = new NavigationModel();
            model.ShowMenuInCustomerNavigation = _helpdeskSettings.ShowMenuInCustomerNavigation;
            return View(model);
        }
    }
}