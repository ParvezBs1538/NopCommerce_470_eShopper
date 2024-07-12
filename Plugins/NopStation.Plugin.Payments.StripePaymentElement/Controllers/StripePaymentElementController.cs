using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Http.Extensions;
using Nop.Core.Infrastructure;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Payments.StripePaymentElement.Models;
using Stripe;

namespace NopStation.Plugin.Payments.StripePaymentElement.Controllers;

public class StripePaymentElementController : NopStationPublicController
{
    #region Field  

    private readonly IStoreContext _storeContext;
    private readonly StripePaymentElementSettings _stripePaymentElementSettings;
    private readonly INopFileProvider _nopFileProvider;
    private readonly ILogger _logger;
    private readonly IWorkContext _workContext;
    private readonly IShoppingCartService _shoppingCartService;
    private readonly IOrderTotalCalculationService _orderTotalCalculationService;
    private readonly IOrderProcessingService _orderProcessingService;
    private readonly IPaymentPluginManager _paymentPluginManager;
    private readonly ICurrencyService _currencyService;
    private readonly IPaymentService _paymentService;
    private readonly ICustomerService _customerService;

    #endregion

    #region Ctor

    public StripePaymentElementController(IStoreContext storeContext,
        StripePaymentElementSettings stripePaymentElementSettings,
        INopFileProvider nopFileProvider,
        ILogger logger,
        IWorkContext workContext,
        IShoppingCartService shoppingCartService,
        IOrderTotalCalculationService orderTotalCalculationService,
        IOrderProcessingService orderProcessingService,
        IPaymentPluginManager paymentPluginManager,
        ICurrencyService currencyService,
        IPaymentService paymentService,
        ICustomerService customerService)
    {
        _storeContext = storeContext;
        _stripePaymentElementSettings = stripePaymentElementSettings;
        _nopFileProvider = nopFileProvider;
        _logger = logger;
        _workContext = workContext;
        _shoppingCartService = shoppingCartService;
        _orderTotalCalculationService = orderTotalCalculationService;
        _orderProcessingService = orderProcessingService;
        _paymentPluginManager = paymentPluginManager;
        _currencyService = currencyService;
        _paymentService = paymentService;
        _customerService = customerService;
    }

    #endregion

    #region Methods

    [HttpPost, CheckLanguageSeoCode(true)]
    public async Task<IActionResult> CreatePaymentIntent()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();
        var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, storeId: store.Id);

        //total
        var (shoppingCartTotalBase, _, _, _, _, _) = await _orderTotalCalculationService.GetShoppingCartTotalAsync(cart);
        var currrency = await _workContext.GetWorkingCurrencyAsync();

        var shoppingCartTotal = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartTotalBase.Value, await _workContext.GetWorkingCurrencyAsync());

        var billingAddress = await _customerService.GetCustomerBillingAddressAsync(customer);

        var options = new PaymentIntentCreateOptions
        {
            Amount = (long)Math.Floor(shoppingCartTotal * 100),
            Currency = currrency.CurrencyCode.ToLower(),
            Description = store.Name,
            ReceiptEmail = billingAddress.Email,
            CaptureMethod = "manual",
            PaymentMethodTypes = new List<string> { "card" },
            PaymentMethodOptions = new PaymentIntentPaymentMethodOptionsOptions
            {
                Card = new PaymentIntentPaymentMethodOptionsCardOptions
                {
                    RequestThreeDSecure = "automatic"
                }
            }
        };

        var service = new PaymentIntentService(new StripeClient(apiKey: _stripePaymentElementSettings.SecretKey));

        try
        {
            var paymentIntent = await service.CreateAsync(options);

            return Json(new CreatePaymentIntentResponse
            {
                Result = "success",
                ClientSecret = paymentIntent.ClientSecret,
            });
        }
        catch (StripeException e)
        {
            return Json(new { result = "fail", error = new { message = e.StripeError.Message } });
        }
        catch (Exception)
        {
            return Json(new { result = "fail", error = new { message = "unknown failure: 500" } });
        }
    }

    public async Task<IActionResult> ConfirmPayment([FromQuery(Name = "payment_intent")] string paymentIntent,
        [FromQuery(Name = "payment_intent_client_secret")] string paymentIntentClientSecret)
    {
        if (!await _paymentPluginManager.IsPluginActiveAsync(StripeDefaults.SystemName))
            return AccessDeniedView();

        if (_stripePaymentElementSettings.EnableLogging)
            await _logger.InformationAsync("StripePaymentElement: Payment request received.\nPayment intent: "
                + paymentIntent + "\nClient secret: " + paymentIntentClientSecret);

        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();

        var paymentIntentService = new PaymentIntentService(new StripeClient(_stripePaymentElementSettings.SecretKey));

        var response = await paymentIntentService.GetAsync(paymentIntent);

        var processPaymentRequest = new ProcessPaymentRequest
        {
            CustomerId = customer.Id,
            StoreId = store.Id,
            PaymentMethodSystemName = StripeDefaults.SystemName,
            OrderGuid = response.Metadata.TryGetValue("orderGuid", out var guid) ? Guid.Parse(guid) : Guid.NewGuid(),
        };

        processPaymentRequest.CustomValues.TryAdd(StripeDefaults.PaymentIntentId, paymentIntent);

        await HttpContext.Session.SetAsync("OrderPaymentInfo", processPaymentRequest);
        var placeOrderResult = await _orderProcessingService.PlaceOrderAsync(processPaymentRequest);
        if (placeOrderResult.Success)
        {
            await HttpContext.Session.SetAsync<ProcessPaymentRequest>("OrderPaymentInfo", null);
            var postProcessPaymentRequest = new PostProcessPaymentRequest
            {
                Order = placeOrderResult.PlacedOrder
            };
            await _paymentService.PostProcessPaymentAsync(postProcessPaymentRequest);

            if (placeOrderResult.Success && placeOrderResult.PlacedOrder != null)
                return RedirectToRoute("CheckoutCompleted", new { orderId = placeOrderResult.PlacedOrder.Id });
        }

        TempData["Errors"] = placeOrderResult.Errors;

        return RedirectToAction("PaymentInfo", "StripePaymentElement");
    }

    public async Task<IActionResult> PaymentInfo()
    {
        await Task.CompletedTask;
        return View();
    }

    [Route(".well-known/apple-developer-merchantid-domain-association")]
    [HttpGet]
    public async Task<IActionResult> RenderMerchantId()
    {
        var storeId = _storeContext.GetCurrentStore().Id;
        var filePath = _nopFileProvider.MapPath(string.Format(StripeDefaults.AppleVerificationFilePath, storeId));

        if (!_nopFileProvider.FileExists(filePath))
            filePath = _nopFileProvider.MapPath(string.Format(StripeDefaults.AppleVerificationFilePath, 0));

        if (!_nopFileProvider.FileExists(filePath))
            return InvokeHttp404();

        return Content(await System.IO.File.ReadAllTextAsync(filePath), "text/plain");
    }

    #endregion
}
