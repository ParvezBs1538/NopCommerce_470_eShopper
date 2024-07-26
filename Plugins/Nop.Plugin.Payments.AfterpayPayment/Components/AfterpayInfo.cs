using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Payments;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Web.Framework.Components;
using Nop.Web.Models.Order;

namespace Nop.Plugin.Payments.AfterpayPayment.Components
{
    [ViewComponent(Name = "AfterpayInfo")]
    public class AfterpayInfoViewComponent : NopViewComponent
    {
        private readonly IOrderService _orderService;
        private readonly ILocalizationService _localizationService;

        public AfterpayInfoViewComponent(IOrderService orderService,
            ILocalizationService localizationService)
        {
            _orderService = orderService;
            _localizationService = localizationService;
        }
        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            var orderDetailsModel = (OrderDetailsModel)additionalData;
            var order = _orderService.GetOrderById(orderDetailsModel.Id);
            if (order == null)
                return Content(string.Empty);
            if (order.PaymentStatus == PaymentStatus.Paid)
            {
                return Content(string.Empty);
            }
            var message = _localizationService.GetResource("Plugins.Payments.Afterpay.Message.Notpaid");
            return View("~/Plugins/Payments.AfterpayPayment/Views/AfterpayInfo.cshtml", message);
        }
    }
}
