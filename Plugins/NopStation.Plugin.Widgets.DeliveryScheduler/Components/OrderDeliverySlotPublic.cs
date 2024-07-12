using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.DeliveryScheduler.Factories;
using NopStation.Plugin.Widgets.DeliveryScheduler.Services;
using Nop.Web.Models.Order;

namespace NopStation.Plugin.Widgets.DeliveryScheduler.Components
{
    public class OrderDeliverySlotPublicViewComponent : NopStationViewComponent
    {
        private readonly DeliverySchedulerSettings _deliverySchedulerSettings;
        private readonly IDeliverySchedulerModelFactory _deliverySchedulerModelFactory;
        private readonly IOrderDeliverySlotService _orderDeliverySlotService;

        public OrderDeliverySlotPublicViewComponent(DeliverySchedulerSettings deliverySchedulerSettings,
            IDeliverySchedulerModelFactory deliverySchedulerModelFactory,
            IOrderDeliverySlotService orderDeliverySlotService)
        {
            _deliverySchedulerSettings = deliverySchedulerSettings;
            _deliverySchedulerModelFactory = deliverySchedulerModelFactory;
            _orderDeliverySlotService = orderDeliverySlotService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            if (!_deliverySchedulerSettings.EnableScheduling)
                return Content("");

            if (additionalData.GetType() != typeof(OrderDetailsModel))
                return Content("");

            var currentModel = additionalData as OrderDetailsModel;
            var orderDeliverySlot = await _orderDeliverySlotService.GetOrderDeliverySlotByOrderId(currentModel.Id);
            if (orderDeliverySlot == null)
                return Content("");

            var model = await _deliverySchedulerModelFactory.PreparedOrderDeliverySlotModelAsync(orderDeliverySlot);
            if (model == null)
                return Content("");

            return View(model);
        }
    }
}
