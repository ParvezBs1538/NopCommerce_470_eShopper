using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Widgets.DeliveryScheduler.Areas.Admin.Models;
using NopStation.Plugin.Widgets.DeliveryScheduler.Services;
using Nop.Web.Areas.Admin.Models.Orders;

namespace NopStation.Plugin.Widgets.DeliveryScheduler.Areas.Admin.Components
{
    public class OrderDeliverySlotViewComponent : NopStationViewComponent
    {
        private readonly IOrderDeliverySlotService _orderDeliverySlotService;
        private readonly IDeliverySlotService _deliverySlotService;

        public OrderDeliverySlotViewComponent(IDeliverySlotService deliverySlotService, 
            IOrderDeliverySlotService orderDeliverySlotService)
        {
            _orderDeliverySlotService = orderDeliverySlotService;
            _deliverySlotService = deliverySlotService;

        }
        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            if (additionalData.GetType() != typeof(OrderModel))
                return Content("");

            var orderModel = additionalData as OrderModel;
            var orderDeliverySlot = await _orderDeliverySlotService.GetOrderDeliverySlotByOrderId(orderModel.Id);
            if (orderDeliverySlot == null)
                return Content("");

            var model = new OrderDeliverySlotModel()
            {
                DeliveryDate = orderDeliverySlot.DeliveryDate,
                DeliverySlotId = orderDeliverySlot.DeliverySlotId,
                Id = orderModel.Id
            };

            var deliverySlots = await _deliverySlotService.SearchDeliverySlotsAsync(active: true);
            model.AvailableDeliverySlots = deliverySlots.Select(x => new SelectListItem()
            {
                Text = x.TimeSlot,
                Value = x.Id.ToString()
            }).ToList();

            return View(model);
        }
    }
}
