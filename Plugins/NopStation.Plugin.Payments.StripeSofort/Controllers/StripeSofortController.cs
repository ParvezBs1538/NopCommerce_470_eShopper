using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Orders;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using Nop.Services.Orders;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Payments.StripeSofort.Services;
using Stripe;

namespace NopStation.Plugin.Payments.StripeSofort.Controllers
{
    public class StripeSofortController : NopStationPublicController
    {
        #region Fields

        private readonly IOrderService _orderService;
        private readonly StripeSofortPaymentSettings _sofortPaymentSettings;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ILogger _logger;
        private readonly ISettingService _settingService;
        private readonly StripeManager _stripeManager;

        #endregion

        #region Ctor

        public StripeSofortController(IOrderService orderService,
            StripeSofortPaymentSettings sofortPaymentSettings,
            IOrderProcessingService orderProcessingService,
            ILogger logger,
            ISettingService settingService,
            StripeManager stripeManager)
        {
            _orderService = orderService;
            _sofortPaymentSettings = sofortPaymentSettings;
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

            var sofortPaymentSettings = await _settingService.LoadSettingAsync<StripeSofortPaymentSettings>(order.StoreId);
            var service = new PaymentIntentService(new StripeClient(apiKey: sofortPaymentSettings.ApiKey));
            var intent = await service.GetAsync(order.AuthorizationTransactionId);

            if (intent.Status.Equals("processing", StringComparison.InvariantCultureIgnoreCase))
            {
                order.AuthorizationTransactionCode = intent.PaymentMethodId;
                await _orderService.UpdateOrderAsync(order);
            }

            return RedirectToRoute("OrderDetails", new { orderId = order.Id });
        }

        [HttpPost]
        public async Task<IActionResult> Webhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(json,
                    Request.Headers["Stripe-Signature"], _sofortPaymentSettings.WebhookSecret);
                var service = new PaymentIntentService(new StripeClient(apiKey: _sofortPaymentSettings.ApiKey));

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
