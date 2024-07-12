using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NopStation.Plugin.Misc.Core.Controllers;
using Nop.Services.Orders;
using Stripe;
using System.IO;
using Nop.Core.Domain.Orders;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using NopStation.Plugin.Payments.StripeGrabPay.Services;

namespace NopStation.Plugin.Payments.StripeGrabPay.Controllers
{
    public class StripeGrabPayController : NopStationPublicController
    {
        #region Fields

        private readonly IOrderService _orderService;
        private readonly StripeGrabPayPaymentSettings _grabPayPaymentSettings;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ISettingService _settingService;
        private readonly ILogger _logger;
        private readonly StripeManager _stripeManager;

        #endregion

        #region Ctor

        public StripeGrabPayController(IOrderService orderService,
            StripeGrabPayPaymentSettings grabPayPaymentSettings,
            IOrderProcessingService orderProcessingService,
            ISettingService settingService,
            ILogger logger,
            StripeManager stripeManager)
        {
            _orderService = orderService;
            _grabPayPaymentSettings = grabPayPaymentSettings;
            _orderProcessingService = orderProcessingService;
            _settingService = settingService;
            _logger = logger;
            _stripeManager = stripeManager;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Callback(int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null || order.Deleted)
                return RedirectToRoute("Homepage");

            var grabPayPaymentSettings = await _settingService.LoadSettingAsync<StripeGrabPayPaymentSettings>(order.StoreId);
            if (!grabPayPaymentSettings.EnableWebhook)
            {
                var service = new PaymentIntentService(new StripeClient(apiKey: grabPayPaymentSettings.ApiKey));
                var intent = await service.GetAsync(order.AuthorizationTransactionId);

                if (intent.AmountReceived >= _stripeManager.ConvertCurrencyToLong(order.OrderTotal, order.CurrencyRate) && _orderProcessingService.CanMarkOrderAsPaid(order))
                {
                    await _orderProcessingService.MarkOrderAsPaidAsync(order);

                    await _orderService.InsertOrderNoteAsync(new OrderNote()
                    {
                        CreatedOnUtc = DateTime.UtcNow,
                        OrderId = order.Id,
                        Note = $"Order payment confirmed by Stripe Alipay. Stripe payment intent identifier: {order.AuthorizationTransactionId}"
                    });
                }
            }

            return RedirectToRoute("OrderDetails", new { orderId = order.Id });
        }

        [HttpPost]
        public async Task<IActionResult> Webhook()
        {
            if (!_grabPayPaymentSettings.EnableWebhook)
                return NotFound();

            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(json,
                    Request.Headers["Stripe-Signature"], _grabPayPaymentSettings.WebhookSecret);
                var service = new PaymentIntentService(new StripeClient(apiKey: _grabPayPaymentSettings.ApiKey));

                var charge = stripeEvent.Data.Object as Charge;
                var intent = await service.GetAsync(charge.PaymentIntentId);

                if (intent.Metadata.TryGetValue(StripeDefaults.OrderId, out var orderIdStr))
                {
                    var order = await _orderService.GetOrderByIdAsync(Convert.ToInt32(orderIdStr));

                    if (stripeEvent.Type == Events.ChargeRefunded)
                    {
                        var refunded = _stripeManager.ConvertCurrencyFromLong(charge.AmountRefunded, order.CurrencyRate);

                        await _orderService.InsertOrderNoteAsync(new OrderNote()
                        {
                            CreatedOnUtc = DateTime.UtcNow,
                            OrderId = order.Id,
                            Note = $"Order refund confirmed ({refunded} {order.CustomerCurrencyCode}). Stripe Charge identifier: {charge.Id}"
                        });
                    }
                    else if (stripeEvent.Type == Events.ChargeSucceeded)
                    {
                        if (intent.AmountReceived >= _stripeManager.ConvertCurrencyToLong(order.OrderTotal, order.CurrencyRate) && _orderProcessingService.CanMarkOrderAsPaid(order))
                        {
                            await _orderProcessingService.MarkOrderAsPaidAsync(order);

                            order.AuthorizationTransactionId = charge.PaymentIntentId;
                            await _orderService.UpdateOrderAsync(order);

                            await _orderService.InsertOrderNoteAsync(new OrderNote()
                            {
                                CreatedOnUtc = DateTime.UtcNow,
                                OrderId = order.Id,
                                Note = $"Order payment confirmed. Stripe Charge identifier: {charge.Id}"
                            });
                        }
                    }
                }

                return Ok();
            }
            catch (StripeException ex)
            {
                await _logger.ErrorAsync(ex.Message, ex);
                return BadRequest();
            }
        }

        #endregion
    }
}
