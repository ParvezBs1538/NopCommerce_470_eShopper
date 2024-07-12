using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Orders;
using Nop.Web.Areas.Admin.Models.Orders;
using NopStation.Plugin.Misc.Core.Components;
using NopStation.Plugin.Shipping.Redx.Areas.Admin.Models;
using NopStation.Plugin.Shipping.Redx.Services;

namespace NopStation.Plugin.Shipping.Redx.Areas.Admin.Components
{
    public class RedxViewComponent : NopStationViewComponent
    {
        private readonly IOrderService _orderService;
        private readonly IRedxShipmentService _redxShipmentService;

        public RedxViewComponent(IOrderService orderService,
            IRedxShipmentService redxShipmentService)
        {
            _orderService = orderService;
            _redxShipmentService = redxShipmentService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            var shipmentModel = additionalData as ShipmentModel;
            var order = await _orderService.GetOrderByIdAsync(shipmentModel.OrderId);
            if (order.ShippingRateComputationMethodSystemName != "NopStation.Plugin.Shipping.Redx")
                return Content("");

            var model = new ShipmentComponentModel();
            model.OrderId = shipmentModel.OrderId;
            model.Id = shipmentModel.Id;  
            
            var redxShipment = await _redxShipmentService.GetRedxShipmentByShipmentIdAsync(shipmentModel.Id);
            model.CanSendShipmentToRedx = redxShipment == null;

            return View(model);
        }
    }
}
