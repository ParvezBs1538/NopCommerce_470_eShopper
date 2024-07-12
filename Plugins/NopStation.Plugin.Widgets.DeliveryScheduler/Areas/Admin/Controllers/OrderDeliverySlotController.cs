using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.Core.Filters;
using NopStation.Plugin.Widgets.DeliveryScheduler.Areas.Admin.Factories;
using NopStation.Plugin.Widgets.DeliveryScheduler.Areas.Admin.Models;
using NopStation.Plugin.Widgets.DeliveryScheduler.Services;
using Nop.Services.Security;

namespace NopStation.Plugin.Widgets.DeliveryScheduler.Areas.Admin.Controllers
{
    public class OrderDeliverySlotController : NopStationAdminController
    {
        private readonly IOrderDeliverySlotService _orderDeliverySlotService;
        private readonly IPermissionService _permissionService;
        private readonly IOrderDeliverySlotModelFactory _orderDeliverySlotModelFactory;

        public OrderDeliverySlotController(IOrderDeliverySlotService orderDeliverySlotService, 
            IPermissionService permissionService,
            IOrderDeliverySlotModelFactory orderDeliverySlotModelFactory)
        {
            _orderDeliverySlotService = orderDeliverySlotService;
            _permissionService = permissionService;
            _orderDeliverySlotModelFactory = orderDeliverySlotModelFactory;
        }

        [EditAccessAjax, HttpPost]
        public async Task<IActionResult> Update(OrderDeliverySlotModel model)
        {
            if (!await _permissionService.AuthorizeAsync(DeliverySchedulerPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var orderDeliverySlot = await _orderDeliverySlotService.GetOrderDeliverySlotByOrderId(model.Id)
                ?? throw new ArgumentNullException("No order delivery slot found with this specific id");

            orderDeliverySlot.DeliveryDate = model.DeliveryDate;
            orderDeliverySlot.DeliverySlotId = model.DeliverySlotId;
            await _orderDeliverySlotService.UpdateOrderDeliverySlot(orderDeliverySlot);

            return Json(new { result = true });
        }

        public async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(DeliverySchedulerPermissionProvider.ManageConfiguration))
                return AccessDeniedView();

            var model = await _orderDeliverySlotModelFactory.PrepareOrderDeliverySlotSearchModelAsync(new OrderDeliverySlotSearchModel());
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> List(OrderDeliverySlotSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(DeliverySchedulerPermissionProvider.ManageConfiguration))
                return await AccessDeniedDataTablesJson();

            var model = await _orderDeliverySlotModelFactory.PrepareOrderDeliverySlotModelListAsync(searchModel);
            return Json(model);
        }
    }
}
