using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Widgets.DeliveryScheduler.Services;
using NopStation.Plugin.Widgets.DeliveryScheduler.Factories;
using Nop.Web.Controllers;

namespace NopStation.Plugin.Widgets.DeliveryScheduler.Controllers
{
    public class DeliverySchedulerController : BasePublicController
    {
        private readonly ICustomShippingService _customShippingService;
        private readonly IDeliverySchedulerModelFactory _deliverySchedulerModelFactory;
        private readonly DeliverySchedulerSettings _deliverySchedulerSettings;

        public DeliverySchedulerController(ICustomShippingService customShippingService,
            IDeliverySchedulerModelFactory deliverySchedulerModelFactory,
            DeliverySchedulerSettings deliverySchedulerSettings)
        {
            _customShippingService = customShippingService;
            _deliverySchedulerModelFactory = deliverySchedulerModelFactory;
            _deliverySchedulerSettings = deliverySchedulerSettings;
        }

        [HttpPost]
        public async Task<IActionResult> DeliverySlots(string methodName, string methodSystemName)
        {
            if (!_deliverySchedulerSettings.EnableScheduling)
                return Json(new { result = false });

            if (methodSystemName != DeliverySchedulerDefaults.ShippingProviderName)
                return Json(new { result = false });

            var shippingMethod = await _customShippingService.GetShippingMethodByNameAsync(methodName);
            if (shippingMethod == null)
                return Json(new { result = false });

            var model = await _deliverySchedulerModelFactory.PrepareDeliverySlotDetailsModel(shippingMethod);
            var html = await RenderPartialViewToStringAsync("DeliverySlots", model);

            return Json(new { result = true, html = html });
        }
    }
}
