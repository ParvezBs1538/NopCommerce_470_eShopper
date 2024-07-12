using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Web.Factories;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Misc.WebApi.Extensions;
using NopStation.Plugin.Misc.WebApi.Models.Checkout;

namespace NopStation.Plugin.Misc.WebApi.Controllers.Web
{
    public class NopStationCheckoutController : NopStationPublicController
    {
        #region Fields

        private readonly ICheckoutModelFactory _checkoutModelFactory;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly OrderSettings _orderSettings;
        private readonly PaymentSettings _paymentSettings;
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;
        private readonly IWebHelper _webHelper;
        private readonly ICustomerService _customerService;

        #endregion

        #region Ctor

        public NopStationCheckoutController(ICheckoutModelFactory checkoutModelFactory,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            IOrderProcessingService orderProcessingService,
            IPaymentPluginManager paymentPluginManager,
            IShoppingCartService shoppingCartService,
            IStoreContext storeContext,
            IWorkContext workContext,
            OrderSettings orderSettings,
            PaymentSettings paymentSettings,
            IOrderService orderService,
            IPaymentService paymentService,
            IWebHelper webHelper,
            ICustomerService customerService)
        {
            _checkoutModelFactory = checkoutModelFactory;
            _genericAttributeService = genericAttributeService;
            _localizationService = localizationService;
            _orderProcessingService = orderProcessingService;
            _paymentPluginManager = paymentPluginManager;
            _shoppingCartService = shoppingCartService;
            _storeContext = storeContext;
            _workContext = workContext;
            _orderSettings = orderSettings;
            _paymentSettings = paymentSettings;
            _orderService = orderService;
            _paymentService = paymentService;
            _webHelper = webHelper;
            _customerService = customerService;
        }

        #endregion

        #region Utilities

        protected virtual async Task GenerateOrderGuid(ProcessPaymentRequest processPaymentRequest)
        {
            if (processPaymentRequest == null)
                return;

            //we should use the same GUID for multiple payment attempts
            //this way a payment gateway can prevent security issues such as credit card brute-force attacks
            //in order to avoid any possible limitations by payment gateway we reset GUID periodically
            var previousPaymentRequest = await _genericAttributeService.GetPaymentRequestAttributeAsync(await _workContext.GetCurrentCustomerAsync(),
                (await _storeContext.GetCurrentStoreAsync()).Id);

            if (_paymentSettings.RegenerateOrderGuidInterval > 0 &&
                previousPaymentRequest != null &&
                previousPaymentRequest.OrderGuidGeneratedOnUtc.HasValue)
            {
                var interval = DateTime.UtcNow - previousPaymentRequest.OrderGuidGeneratedOnUtc.Value;
                if (interval.TotalSeconds < _paymentSettings.RegenerateOrderGuidInterval)
                {
                    processPaymentRequest.OrderGuid = previousPaymentRequest.OrderGuid;
                    processPaymentRequest.OrderGuidGeneratedOnUtc = previousPaymentRequest.OrderGuidGeneratedOnUtc;
                }
            }

            if (processPaymentRequest.OrderGuid == Guid.Empty)
            {
                processPaymentRequest.OrderGuid = Guid.NewGuid();
                processPaymentRequest.OrderGuidGeneratedOnUtc = DateTime.UtcNow;
            }
        }

        #endregion

        #region Methods

        public IActionResult Step(int nextStep)
        {
            return Content("");
        }

        public async Task<IActionResult> PaymentInfo()
        {
            //validation
            if (_orderSettings.CheckoutDisabled)
                return RedirectToRoute("NopStationCheckoutStep", new { nextStep = (int)OpcStep.CartPage });

            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

            if (!cart.Any())
                return RedirectToRoute("NopStationCheckoutStep", new { nextStep = (int)OpcStep.CartPage });

            if (await _customerService.IsGuestAsync(customer) && !_orderSettings.AnonymousCheckoutAllowed)
                return RedirectToRoute("NopStationCheckoutStep", new { nextStep = (int)OpcStep.CartPage });

            //Check whether payment workflow is required
            var isPaymentWorkflowRequired = await _orderProcessingService.IsPaymentWorkflowRequiredAsync(cart);
            if (!isPaymentWorkflowRequired)
            {
                return RedirectToRoute("NopStationCheckoutStep", new { nextStep = (int)OpcStep.ConfirmOrder });
            }

            //load payment method
            var paymentMethodSystemName = await _genericAttributeService.GetAttributeAsync<string>(customer,
                NopCustomerDefaults.SelectedPaymentMethodAttribute, store.Id);
            var paymentMethod = await _paymentPluginManager.LoadPluginBySystemNameAsync(paymentMethodSystemName);
            if (paymentMethod == null)
                return RedirectToRoute("NopStationCheckoutStep", new { nextStep = (int)OpcStep.PaymentMethod });

            //Check whether payment info should be skipped
            if (paymentMethod.SkipPaymentInfo ||
                (paymentMethod.PaymentMethodType == PaymentMethodType.Redirection && _paymentSettings.SkipPaymentInfoStepForRedirectionPaymentMethods))
            {
                //skip payment info page
                var paymentInfo = new ProcessPaymentRequest();

                //session save
                await _genericAttributeService.SavePaymentRequestAttributeAsync(customer, paymentInfo, store.Id);

                return RedirectToRoute("NopStationCheckoutStep", new { nextStep = (int)OpcStep.ConfirmOrder });
            }

            //model
            var model = await _checkoutModelFactory.PreparePaymentInfoModelAsync(paymentMethod);
            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> PaymentInfo(IFormCollection form)
        {
            //validation
            if (_orderSettings.CheckoutDisabled)
                return RedirectToRoute("NopStationCheckoutStep", new { nextStep = (int)OpcStep.CartPage });

            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();
            var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, store.Id);

            if (!cart.Any())
                return RedirectToRoute("NopStationCheckoutStep", new { nextStep = (int)OpcStep.CartPage });

            if (await _customerService.IsGuestAsync(customer) && !_orderSettings.AnonymousCheckoutAllowed)
                return Challenge();

            //Check whether payment workflow is required
            var isPaymentWorkflowRequired = await _orderProcessingService.IsPaymentWorkflowRequiredAsync(cart);
            if (!isPaymentWorkflowRequired)
            {
                return RedirectToRoute("NopStationCheckoutStep", new { nextStep = (int)OpcStep.ConfirmOrder });
            }

            //load payment method
            var paymentMethodSystemName = await _genericAttributeService.GetAttributeAsync<string>(customer,
                NopCustomerDefaults.SelectedPaymentMethodAttribute, store.Id);
            var paymentMethod = await _paymentPluginManager.LoadPluginBySystemNameAsync(paymentMethodSystemName);
            if (paymentMethod == null)
                return RedirectToRoute("NopStationCheckoutStep", new { nextStep = (int)OpcStep.PaymentMethod });

            var warnings = await paymentMethod.ValidatePaymentFormAsync(form);
            foreach (var warning in warnings)
                ModelState.AddModelError("", warning);
            if (ModelState.IsValid)
            {
                //get payment info
                var paymentInfo = await paymentMethod.GetPaymentInfoAsync(form);
                //set previous order GUID (if exists)
                await GenerateOrderGuid(paymentInfo);

                //session save
                await _genericAttributeService.SavePaymentRequestAttributeAsync(customer, paymentInfo, store.Id);

                return RedirectToRoute("NopStationCheckoutStep", new { nextStep = (int)OpcStep.ConfirmOrder });
            }

            //If we got this far, something failed, redisplay form
            //model
            var model = await _checkoutModelFactory.PreparePaymentInfoModelAsync(paymentMethod);
            return View(model);
        }

        public async Task<IActionResult> Redirect(int? orderId)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            var store = await _storeContext.GetCurrentStoreAsync();

            //validation
            if (await _customerService.IsGuestAsync(customer) && !_orderSettings.AnonymousCheckoutAllowed)
                return Challenge();

            //get the order
            var order = orderId.HasValue ? await _orderService.GetOrderByIdAsync(orderId.Value) : (await _orderService.SearchOrdersAsync(storeId: store.Id,
                    customerId: customer.Id, pageSize: 1)).FirstOrDefault();

            if (order == null)
                return RedirectToRoute("Homepage");

            var paymentMethod = await _paymentPluginManager.LoadPluginBySystemNameAsync(order.PaymentMethodSystemName);
            if (paymentMethod == null)
                return RedirectToRoute("Homepage");
            if (paymentMethod.PaymentMethodType != PaymentMethodType.Redirection)
                return RedirectToRoute("Homepage");

            //ensure that order has been just placed
            if ((DateTime.UtcNow - order.CreatedOnUtc).TotalMinutes > 3)
                return RedirectToRoute("Homepage");

            //Redirection will not work on one page checkout page because it's AJAX request.
            //That's why we process it here
            var postProcessPaymentRequest = new PostProcessPaymentRequest
            {
                Order = order
            };

            await _paymentService.PostProcessPaymentAsync(postProcessPaymentRequest);

            if (_webHelper.IsRequestBeingRedirected || _webHelper.IsPostBeingDone)
            {
                //redirection or POST has been done in PostProcessPayment
                return Content("Redirected");
            }

            //if no redirection has been done (to a third-party payment page)
            //theoretically it's not possible
            return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });
        }

        public virtual async Task<IActionResult> RePostPayment(int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order == null || order.Deleted || (await _workContext.GetCurrentCustomerAsync()).Id != order.CustomerId)
                return RedirectToRoute("Homepage");

            if (!await _paymentService.CanRePostProcessPaymentAsync(order))
                return RedirectToRoute("Homepage");

            var postProcessPaymentRequest = new PostProcessPaymentRequest
            {
                Order = order
            };
            await _paymentService.PostProcessPaymentAsync(postProcessPaymentRequest);

            if (_webHelper.IsRequestBeingRedirected || _webHelper.IsPostBeingDone)
            {
                //redirection or POST has been done in PostProcessPayment
                return Content("Redirected");
            }

            //if no redirection has been done (to a third-party payment page)
            //theoretically it's not possible
            return RedirectToRoute("OrderDetails", new { orderId = orderId });
        }

        #endregion
    }
}
