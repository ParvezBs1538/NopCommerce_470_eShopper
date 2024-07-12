using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Orders;
using Nop.Web.Framework.Controllers;
using NopStation.Plugin.Payments.SSLCommerz.Sevices;

namespace NopStation.Plugin.Payments.SSLCommerz.Controllers
{
    public class SSLCommerzController : BasePaymentController
    {
        #region Fields

        private readonly ISettingService _settingService;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ISSLCommerzManager _commerzManager;

        #endregion

        #region Ctor

        public SSLCommerzController(IWorkContext workContext,
            ISettingService settingService,
            IOrderService orderService,
            IOrderProcessingService orderProcessingService,
            ISSLCommerzManager commerzManager)
        {
            _settingService = settingService;
            _orderService = orderService;
            _orderProcessingService = orderProcessingService;
            _commerzManager = commerzManager;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Success(Guid orderGuid)
        {
            var order = await _orderService.GetOrderByGuidAsync(orderGuid);
            if (order == null || order.Deleted)
                return RedirectToRoute("Homepage");

            var result = await _commerzManager.ValidatePaymentAsync(order);

            if (result.Success)
            {
                if (_orderProcessingService.CanMarkOrderAsPaid(order))
                    await _orderProcessingService.MarkOrderAsPaidAsync(order);
            }

            return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });
        }

        public async Task<IActionResult> Failed(Guid orderGuid)
        {
            var order = await _orderService.GetOrderByGuidAsync(orderGuid);
            if (order == null || order.Deleted)
                return RedirectToRoute("HomePage");

            return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });
        }

        #endregion
    }
}