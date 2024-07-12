using System;
using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.Helpdesk.Models;

namespace NopStation.Plugin.Widgets.Helpdesk.Components
{
    public class HelpdeskOrderViewComponent : NopStationViewComponent
    {
        private readonly HelpdeskSettings _helpdeskSettings;

        public HelpdeskOrderViewComponent(HelpdeskSettings helpdeskSettings)
        {
            _helpdeskSettings = helpdeskSettings;
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            var orderId = Convert.ToInt32(Url.ActionContext.RouteData.Values["orderId"].ToString());
            var model = new OrderModel
            {
                AllowCustomerToCreateTicketFromOrderPage = _helpdeskSettings.AllowCustomerToCreateTicketFromOrderPage,
                OrderId = orderId
            };

            return View(model);
        }
    }
}