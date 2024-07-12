using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Pickup.PickupInStoreAdvance.Areas.Admin.Models;
using NopStation.Plugin.Pickup.PickupInStoreAdvance.Domain.Enum;
using NopStation.Plugin.Pickup.PickupInStoreAdvance.Services;
using Nop.Web.Areas.Admin.Models.Orders;

namespace NopStation.Plugin.Pickup.PickupInStoreAdvance.Areas.Admin.Components
{
    public class MarkReadyOrPickedViewComponent : NopStationViewComponent
    {
        private readonly IPickupInStoreDeliveryManageService _pickupInStoreDeliveryManageService;

        public MarkReadyOrPickedViewComponent(IPickupInStoreDeliveryManageService pickupInStoreDeliveryManageService)
        {
            _pickupInStoreDeliveryManageService = pickupInStoreDeliveryManageService;
        }
        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            var orderModel = (OrderModel)additionalData;
            if (orderModel == null)
            {
                return Content(string.Empty);
            }
            var deliveryManage = await _pickupInStoreDeliveryManageService.GetPickupInStoreDeliverManageByOrderIdAsync(orderModel.Id);
            if (deliveryManage == null || deliveryManage.PickUpStatusTypeId == (int)PickUpStatusType.OrderCanceled)
            {
                return Content(string.Empty);
            }
            var model = new MarkedReadyOrPickedModel
            {
                OrderId = orderModel.Id,
                StatusId = deliveryManage.PickUpStatusTypeId,
            };
            return View(model);
        }
    }
}
