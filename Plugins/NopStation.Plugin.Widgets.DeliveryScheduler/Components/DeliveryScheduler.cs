using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.DeliveryScheduler.Services;

namespace NopStation.Plugin.Widgets.DeliveryScheduler.Components
{
    public class DeliverySchedulerViewComponent : NopStationViewComponent
    {
        private readonly DeliverySchedulerSettings _deliverySchedulerSettings;
        private readonly IDeliverySlotService _deliverySlotService;

        public DeliverySchedulerViewComponent(DeliverySchedulerSettings deliverySchedulerSettings,
            IDeliverySlotService deliverySlotService)
        {
            _deliverySchedulerSettings = deliverySchedulerSettings;
            _deliverySlotService = deliverySlotService;
        }

        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            if (!_deliverySchedulerSettings.EnableScheduling)
                return Content("");

            if (!_deliverySlotService.IsActiveDeliverySlotExits())
                return Content("");


            return View();
        }
    }
}
