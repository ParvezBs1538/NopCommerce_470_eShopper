using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Logging;
using Nop.Services.Orders;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Payments.StripeOxxo.Models;
using Stripe;
using Address = Nop.Core.Domain.Common.Address;
using Order = Nop.Core.Domain.Orders.Order;

namespace NopStation.Plugin.Payments.StripeOxxo.Controllers
{
    public class StripeOxxoController : NopStationPublicController
    {
        #region Fields

        private readonly IOrderService _orderService;
        private readonly StripeOxxoPaymentSettings _oxxoPaymentSettings;
        private readonly ICurrencyService _currencyService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IWorkContext _workContext;
        private readonly ILogger _logger;
        private readonly ISettingService _settingService;
        private readonly IAddressService _addressService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ICountryService _countryService;

        #endregion

        #region Ctor

        public StripeOxxoController(IOrderService orderService,
            StripeOxxoPaymentSettings oxxoPaymentSettings,
            ICurrencyService currencyService,
            IOrderProcessingService orderProcessingService,
            IWorkContext workContext,
            ILogger logger,
            ISettingService settingService,
            IAddressService addressService,
            IStateProvinceService stateProvinceService,
            ICountryService countryService)
        {
            _orderService = orderService;
            _oxxoPaymentSettings = oxxoPaymentSettings;
            _currencyService = currencyService;
            _orderProcessingService = orderProcessingService;
            _workContext = workContext;
            _logger = logger;
            _settingService = settingService;
            _addressService = addressService;
            _stateProvinceService = stateProvinceService;
            _countryService = countryService;
        }

        #endregion

        #region Utilities

        protected async Task PrepareOrderDetailsAsync(PaymentIntentCreateOptions options, Order order)
        {
            var address = await _addressService.GetAddressByIdAsync(order.BillingAddressId);
            options.ReceiptEmail = address.Email;

            if (order.ShippingStatus != ShippingStatus.ShippingNotRequired && await _addressService.GetAddressByIdAsync(order.ShippingAddressId ?? 0) is Address shippingAddress)
            {
                options.Shipping = new ChargeShippingOptions()
                {
                    Address = new AddressOptions()
                    {
                        City = shippingAddress.City,
                        Line1 = shippingAddress.Address1,
                        Line2 = shippingAddress.Address2,
                        PostalCode = shippingAddress.ZipPostalCode,
                        Country = (await _countryService.GetCountryByAddressAsync(shippingAddress))?.TwoLetterIsoCode,
                        State = (await _stateProvinceService.GetStateProvinceByAddressAsync(shippingAddress))?.Name
                    },
                    Name = $"{shippingAddress.FirstName} {shippingAddress.LastName}",
                    Carrier = order.ShippingMethod,
                    Phone = shippingAddress.PhoneNumber,
                };
            }
        }

        protected bool IsValidForPayment(Order order)
        {
            if (order.PaymentMethodSystemName != OxxoDefaults.SystemName)
                return false;

            if (order.OrderStatus == OrderStatus.Cancelled || order.PaymentStatus != PaymentStatus.Pending)
                return false;

            if (!string.IsNullOrWhiteSpace(order.AuthorizationTransactionId))
                return false;

            return true;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Payment(int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null || order.Deleted)
                return RedirectToRoute("Homepage");

            var customer = await _workContext.GetCurrentCustomerAsync();
            if (order.CustomerId != customer.Id)
                return Challenge();

            var oxxoPaymentSettings = await _settingService.LoadSettingAsync<StripeOxxoPaymentSettings>(order.StoreId);
            if (!IsValidForPayment(order))
                return RedirectToRoute("OrderDetails", new { orderId = order.Id });

            StripeConfiguration.ApiKey = oxxoPaymentSettings.ApiKey;
            var amount = _currencyService.ConvertCurrency(order.OrderTotal, order.CurrencyRate);

            var options = new PaymentIntentCreateOptions
            {
                PaymentMethodTypes = new List<string> { "oxxo" },
                Amount = Convert.ToInt64(amount * 100),
                Currency = order.CustomerCurrencyCode.ToLower(),
                Metadata = new Dictionary<string, string> { [OxxoDefaults.OrderId] = order.Id.ToString() }
            };

            if (oxxoPaymentSettings.SendOrderInfoToStripe)
                await PrepareOrderDetailsAsync(options, order);

            var service = new PaymentIntentService();
            var paymentIntent = service.Create(options);

            order.AuthorizationTransactionCode = paymentIntent.ClientSecret;
            await _orderService.UpdateOrderAsync(order);

            var address = await _addressService.GetAddressByIdAsync(order.BillingAddressId);

            var model = new PaymentModel()
            {
                ClientSecret = paymentIntent.ClientSecret,
                Id = order.Id,
                Email = address.Email,
                Name = $"{address.FirstName} {address.LastName}",
                PublishableKey = oxxoPaymentSettings.PublishableKey
            };

            return View("~/Plugins/NopStation.Plugin.Payments.StripeOxxo/Views/Payment.cshtml", model);
        }

        public async Task<IActionResult> Callback(int orderId, string paymentIntentId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null || order.Deleted)
                return RedirectToRoute("Homepage");

            var oxxoPaymentSettings = await _settingService.LoadSettingAsync<StripeOxxoPaymentSettings>(order.StoreId);
            var service = new PaymentIntentService(new StripeClient(apiKey: oxxoPaymentSettings.ApiKey));
            var intent = await service.GetAsync(paymentIntentId);

            if (intent.Metadata.TryGetValue(OxxoDefaults.OrderId, out var orderIdStr) && orderIdStr.Equals(order.Id.ToString()))
            {
                if (intent.Status.Equals("requires_action", StringComparison.InvariantCultureIgnoreCase))
                {
                    order.AuthorizationTransactionId = paymentIntentId;
                    await _orderService.UpdateOrderAsync(order);
                }
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
                    Request.Headers["Stripe-Signature"], _oxxoPaymentSettings.WebhookSecret);
                var service = new PaymentIntentService(new StripeClient(apiKey: _oxxoPaymentSettings.ApiKey));

                if (stripeEvent.Type == Events.ChargeRefunded)
                {
                    var charge = stripeEvent.Data.Object as Charge;
                    var intent = await service.GetAsync(charge.PaymentIntentId);

                    if (intent.Metadata.TryGetValue(OxxoDefaults.OrderId, out var orderIdStr))
                    {
                        var order = await _orderService.GetOrderByIdAsync(Convert.ToInt32(orderIdStr));
                        var refunded = _currencyService.ConvertCurrency(charge.AmountRefunded / 100, 1 / order.CurrencyRate);

                        await _orderService.InsertOrderNoteAsync(new OrderNote()
                        {
                            CreatedOnUtc = DateTime.UtcNow,
                            OrderId = order.Id,
                            Note = $"Order refund confirmed ({refunded} {order.CustomerCurrencyCode}). Stripe Charge identifier: {charge.Id}"
                        });
                    }
                }
                else if (stripeEvent.Type == Events.ChargeSucceeded)
                {
                    var charge = stripeEvent.Data.Object as Charge;
                    var intent = await service.GetAsync(charge.PaymentIntentId);

                    if (intent.Metadata.TryGetValue(OxxoDefaults.OrderId, out var orderIdStr))
                    {
                        var order = await _orderService.GetOrderByIdAsync(Convert.ToInt32(orderIdStr));
                        var amount = _currencyService.ConvertCurrency(order.OrderTotal, order.CurrencyRate);
                        if (intent.AmountReceived >= amount * 100 && _orderProcessingService.CanMarkOrderAsPaid(order))
                        {
                            await _orderProcessingService.MarkOrderAsPaidAsync(order);
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
