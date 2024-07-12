using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using NopStation.Plugin.Widgets.CancelOrder.Models;
using NopStation.Plugin.Misc.Core.Components;
using Nop.Services.Orders;
using Nop.Web.Models.Order;

namespace NopStation.Plugin.Widgets.CancelOrder.Components
{
    public class CancelOrderViewComponent : NopStationViewComponent
    {
        private readonly IOrderService _orderService;
        private readonly IWorkContext _workContext;
        private readonly CancelOrderSettings _cancelOrderSettings;

        public CancelOrderViewComponent(IOrderService orderService,
            IWorkContext workContext,
            CancelOrderSettings cancelOrderSettings)
        {
            _orderService = orderService;
            _workContext = workContext;
            _cancelOrderSettings = cancelOrderSettings;
        }

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            int orderId;
            if (additionalData.GetType() == typeof(OrderDetailsModel))
            {
                var orderModel = additionalData as OrderDetailsModel;
                orderId = orderModel.Id;
            }
            else if (!int.TryParse(additionalData.ToString(), out orderId))
                return Content("");

            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null || order.Deleted)
                return Content("");

            if (order.CustomerId != (await _workContext.GetCurrentCustomerAsync()).Id)
                return Content("");

            if(!_cancelOrderSettings.CancellableOrderStatuses.Contains(order.OrderStatusId)||
                !_cancelOrderSettings.CancellablePaymentStatuses.Contains(order.PaymentStatusId)||
                !_cancelOrderSettings.CancellableShippingStatuses.Contains(order.ShippingStatusId))
                return Content("");

            var model = new PublicInfoModel() { OrderId = orderId };
            return View(model);
        }
    }
}
