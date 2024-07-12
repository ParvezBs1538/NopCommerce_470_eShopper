using System;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Pay.API;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Services.Configuration;
using Nop.Services.Orders;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Payments.AmazonPay.Models;
using NopStation.Plugin.Payments.AmazonPay.Services;
using static NopStation.Plugin.Payments.AmazonPay.Models.PaymentScriptEnum;

namespace NopStation.Plugin.Payments.AmazonPay.Controllers
{
    public class AmazonPayController : NopStationPublicController
    {
        private readonly IOrderService _orderService;
        private readonly IWorkContext _workContext;
        private readonly AmazonManager _amazonManager;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;

        public AmazonPayController(IOrderService orderService,
            IWorkContext workContext,
            AmazonManager amazonManager,
            ISettingService settingService,
            IStoreContext storeContext)
        {
            _orderService = orderService;
            _workContext = workContext;
            _amazonManager = amazonManager;
            _settingService = settingService;
            _storeContext = storeContext;
        }

        public async Task<IActionResult> Redirect(System.Guid orderGuid)
        {
            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetCurrentStoreAsync();
            var amazonPaySettings = await _settingService.LoadSettingAsync<AmazonPaySettings>(storeScope.Id);

            var order = await _orderService.GetOrderByGuidAsync(orderGuid);
            if (order == null || order.Deleted)
                return RedirectToRoute("Homepage");

            var customer = await _workContext.GetCurrentCustomerAsync();
            if (order.CustomerId != customer.Id)
                return Challenge();

            if (order.OrderStatus == OrderStatus.Cancelled || order.PaymentStatus != PaymentStatus.Pending)
                return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });

            var initOrder = await _amazonManager.InitializeOrderAsync(order);

            var scriptRegions = Enum.GetValues(typeof(ScriptRegions))
                                     .Cast<ScriptRegions>()
                                     .ToArray();
            var model = new RedirectModel()
            {
                PublicKeyId = initOrder.AmazonPaySettings.PublicKeyId,
                ButtonColor = initOrder.AmazonPaySettings.ButtonColor,
                Payload = initOrder.Payload,
                Signature = initOrder.Signature,
                OrderGuid = orderGuid,
                Currency = order.CustomerCurrencyCode.ToUpper(),
                Sandbox = initOrder.AmazonPaySettings.UseSandbox,
                MerchantId = initOrder.AmazonPaySettings.MerchantId,
                AmazonSignatureAlgorithm = Constants.AmazonSignatureAlgorithm,
                PaymentScript = string.Format("https://static-{0}.payments-amazon.com/checkout.js", scriptRegions[amazonPaySettings.RegionId])
            };

            return View("~/Plugins/NopStation.Plugin.Payments.AmazonPay/Views/Redirect.cshtml", model);
        }

        public async Task<IActionResult> Callback(System.Guid orderGuid, string amazonCheckoutSessionId)
        {
            var order = await _orderService.GetOrderByGuidAsync(orderGuid);
            if (order == null || order.Deleted)
                return RedirectToRoute("Homepage");

            if (order.OrderStatus == OrderStatus.Cancelled || order.PaymentStatus != PaymentStatus.Pending)
                return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });

            await _amazonManager.CompleteCheckoutSessionAsync(order, amazonCheckoutSessionId);

            return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });
        }
    }
}
