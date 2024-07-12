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
using NopStation.Plugin.Payments.StripeAlipay.Services;

namespace NopStation.Plugin.Payments.StripeAlipay.Controllers
{
    public class StripeAlipayController : NopStationPublicController
    {
        #region Fields

        private readonly IOrderService _orderService;
        private readonly StripeAlipayPaymentSettings _alipayPaymentSettings;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ILogger _logger;
        private readonly ISettingService _settingService;
        private readonly StripeManager _stripeManager;

        #endregion

        #region Ctor

        public StripeAlipayController(IOrderService orderService,
            StripeAlipayPaymentSettings alipayPaymentSettings,
            IOrderProcessingService orderProcessingService,
            ILogger logger,
            ISettingService settingService,
            StripeManager stripeManager)
        {
            _orderService = orderService;
            _alipayPaymentSettings = alipayPaymentSettings;
            _orderProcessingService = orderProcessingService;
            _logger = logger;
            _settingService = settingService;
            _stripeManager = stripeManager;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Callback(int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null || order.Deleted)
                return RedirectToRoute("Homepage");

            try
            {
                var alipayPaymentSettings = await _settingService.LoadSettingAsync<StripeAlipayPaymentSettings>(order.StoreId);
                if (!alipayPaymentSettings.EnableWebhook)
                {
                    var service = new PaymentIntentService(new StripeClient(apiKey: alipayPaymentSettings.ApiKey));
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
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync(ex.Message, ex);
            }

            return RedirectToRoute("OrderDetails", new { orderId = order.Id });
        }

        [HttpPost]
        public async Task<IActionResult> Webhook()
        {
            if (!_alipayPaymentSettings.EnableWebhook)
                return NotFound();

            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(json,
                    Request.Headers["Stripe-Signature"], _alipayPaymentSettings.WebhookSecret);
                var service = new PaymentIntentService(new StripeClient(apiKey: _alipayPaymentSettings.ApiKey));

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
