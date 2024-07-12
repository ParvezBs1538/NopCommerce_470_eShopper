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
using Nop.Services.Payments;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Payments.StripeKonbini.Models;
using Stripe;
using Address = Nop.Core.Domain.Common.Address;
using Order = Nop.Core.Domain.Orders.Order;

namespace NopStation.Plugin.Payments.StripeKonbini.Controllers
{
    public class StripeKonbiniController : NopStationPublicController
    {
        #region Fields

        private readonly IOrderService _orderService;
        private readonly StripeKonbiniPaymentSettings _konbiniPaymentSettings;
        private readonly ICurrencyService _currencyService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IWorkContext _workContext;
        private readonly ILogger _logger;
        private readonly IWebHelper _webHelper;
        private readonly ISettingService _settingService;
        private readonly IPaymentService _paymentService;
        private readonly IAddressService _addressService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ICountryService _countryService;

        #endregion

        #region Ctor

        public StripeKonbiniController(IOrderService orderService,
            StripeKonbiniPaymentSettings konbiniPaymentSettings,
            ICurrencyService currencyService,
            IOrderProcessingService orderProcessingService,
            IWorkContext workContext,
            ILogger logger,
            IWebHelper webHelper,
            ISettingService settingService,
            IPaymentService paymentService,
            IAddressService addressService,
            IStateProvinceService stateProvinceService,
            ICountryService countryService)
        {
            _orderService = orderService;
            _konbiniPaymentSettings = konbiniPaymentSettings;
            _currencyService = currencyService;
            _orderProcessingService = orderProcessingService;
            _workContext = workContext;
            _logger = logger;
            _webHelper = webHelper;
            _settingService = settingService;
            _paymentService = paymentService;
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

        protected bool IsValidForPayment(Order order, Dictionary<string, object> customAttributes)
        {
            if (order.PaymentMethodSystemName != KonbiniDefaults.SystemName)
                return false;

            if (!customAttributes.ContainsKey(KonbiniDefaults.ConfirmationNumber))
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

            var customAttributes = _paymentService.DeserializeCustomValues(order);
            var konbiniPaymentSettings = await _settingService.LoadSettingAsync<StripeKonbiniPaymentSettings>(order.StoreId);
            if (!IsValidForPayment(order, customAttributes))
                return RedirectToRoute("OrderDetails", new { orderId = order.Id });

            StripeConfiguration.ApiKey = konbiniPaymentSettings.ApiKey;
            var amount = _currencyService.ConvertCurrency(order.OrderTotal, order.CurrencyRate);

            var options = new PaymentIntentCreateOptions
            {
                PaymentMethodTypes = new List<string> { "konbini" },
                Amount = Convert.ToInt64(amount),
                Currency = order.CustomerCurrencyCode.ToLower(),
                Metadata = new Dictionary<string, string> { [KonbiniDefaults.OrderId] = order.Id.ToString() }
            };

            if (konbiniPaymentSettings.SendOrderInfoToStripe)
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
                PublishableKey = konbiniPaymentSettings.PublishableKey
            };

            if (customAttributes.TryGetValue(KonbiniDefaults.ConfirmationNumber, out var confirmationNumberValue))
                model.ConfirmationNumber = confirmationNumberValue.ToString();

            return View("~/Plugins/NopStation.Plugin.Payments.StripeKonbini/Views/Payment.cshtml", model);
        }

        public async Task<IActionResult> Callback(int orderId, string paymentIntentId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null || order.Deleted)
                return RedirectToRoute("Homepage");

            var konbiniPaymentSettings = await _settingService.LoadSettingAsync<StripeKonbiniPaymentSettings>(order.StoreId);
            var service = new PaymentIntentService(new StripeClient(apiKey: konbiniPaymentSettings.ApiKey));
            var intent = await service.GetAsync(paymentIntentId);

            if (intent.Metadata.TryGetValue(KonbiniDefaults.OrderId, out var orderIdStr) && orderIdStr.Equals(order.Id.ToString()))
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
                    Request.Headers["Stripe-Signature"], _konbiniPaymentSettings.WebhookSecret);
                var service = new PaymentIntentService(new StripeClient(apiKey: _konbiniPaymentSettings.ApiKey));

                if (stripeEvent.Type == Events.ChargeRefunded)
                {
                    var charge = stripeEvent.Data.Object as Charge;
                    var intent = await service.GetAsync(charge.PaymentIntentId);

                    if (intent.Metadata.TryGetValue(KonbiniDefaults.OrderId, out var orderIdStr))
                    {
                        var order = await _orderService.GetOrderByIdAsync(Convert.ToInt32(orderIdStr));
                        var refunded = _currencyService.ConvertCurrency(charge.AmountRefunded, 1 / order.CurrencyRate);

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

                    if (intent.Metadata.TryGetValue(KonbiniDefaults.OrderId, out var orderIdStr))
                    {
                        var order = await _orderService.GetOrderByIdAsync(Convert.ToInt32(orderIdStr));
                        var amount = _currencyService.ConvertCurrency(order.OrderTotal, order.CurrencyRate);
                        if (intent.AmountReceived >= Convert.ToInt64(amount) && _orderProcessingService.CanMarkOrderAsPaid(order))
                        {
                            await _orderProcessingService.MarkOrderAsPaidAsync(order);

                            order.CaptureTransactionId = charge.Id;
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
