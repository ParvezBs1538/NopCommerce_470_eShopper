using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Orders;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using Nop.Services.Orders;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Payments.StripeKlarna.Services;
using Stripe;

namespace NopStation.Plugin.Payments.StripeKlarna.Controllers
{
    public class StripeKlarnaController : NopStationPublicController
    {
        #region Fields

        private readonly IOrderService _orderService;
        private readonly StripeKlarnaPaymentSettings _klarnaPaymentSettings;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ILogger _logger;
        private readonly ISettingService _settingService;
        private readonly StripeManager _stripeManager;

        #endregion

        #region Ctor

        public StripeKlarnaController(IOrderService orderService,
            StripeKlarnaPaymentSettings klarnaPaymentSettings,
            IOrderProcessingService orderProcessingService,
            ILogger logger,
            ISettingService settingService,
            StripeManager stripeManager)
        {
            _orderService = orderService;
            _klarnaPaymentSettings = klarnaPaymentSettings;
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

            var klarnaPaymentSettings = await _settingService.LoadSettingAsync<StripeKlarnaPaymentSettings>(order.StoreId);

            if (!klarnaPaymentSettings.EnableWebhook)
            {
                var service = new PaymentIntentService(new StripeClient(apiKey: klarnaPaymentSettings.ApiKey));
                var intent = await service.GetAsync(order.AuthorizationTransactionId);

                if (intent.AmountReceived >= _stripeManager.ConvertCurrencyToLong(order.OrderTotal, order.CurrencyRate) && _orderProcessingService.CanMarkOrderAsPaid(order))
                {
                    await _orderProcessingService.MarkOrderAsPaidAsync(order);

                    await _orderService.InsertOrderNoteAsync(new OrderNote()
                    {
                        CreatedOnUtc = DateTime.UtcNow,
                        OrderId = order.Id,
                        Note = $"Order payment confirmed by Stripe Klarna. Stripe payment intent identifier: {order.AuthorizationTransactionId}"
                    });
                }
            }

            return RedirectToRoute("OrderDetails", new { orderId = order.Id });
        }

        [HttpPost]
        public async Task<IActionResult> Webhook()
        {
            if (!_klarnaPaymentSettings.EnableWebhook)
                return NotFound();

            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(json,
                    Request.Headers["Stripe-Signature"], _klarnaPaymentSettings.WebhookSecret);
                var service = new PaymentIntentService(new StripeClient(apiKey: _klarnaPaymentSettings.ApiKey));

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
