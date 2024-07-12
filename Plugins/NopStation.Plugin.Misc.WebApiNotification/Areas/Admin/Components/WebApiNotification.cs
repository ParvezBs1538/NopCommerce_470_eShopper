using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Misc.WebApi.Areas.Admin.Models;

namespace NopStation.Plugin.Misc.WebApiNotification.Areas.Admin.Components
{
    public class WebApiNotificationViewComponent : NopStationViewComponent
    {
        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            if (additionalData.GetType() != typeof(DeviceModel))
                return Content("");

            var model = (DeviceModel)additionalData;
            if (model.Id == 0)
                return Content("");

            return View(model);
        }
    }
}
