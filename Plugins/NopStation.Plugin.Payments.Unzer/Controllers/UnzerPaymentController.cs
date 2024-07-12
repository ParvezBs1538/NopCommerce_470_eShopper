using System;
using System.Linq;
using System.Threading.Tasks;
using Heidelpay.Payment;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Web.Framework.Controllers;
using NopStation.Plugin.Payments.Unzer.Domain;
using NopStation.Plugin.Payments.Unzer.Services;

namespace NopStation.Plugin.Payments.Unzer.Controllers
{
    public class UnzerPaymentController : BasePaymentController
    {
        #region Fields

        private readonly UnzerPaymentSettings _unzerPaymentSettings;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly ILogger _logger;
        private readonly IWebHelper _webHelper;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IUnzerHelperService _unzerHelperService;
        private readonly ICustomerService _customerService;

        #endregion

        #region Ctor

        public UnzerPaymentController(
            UnzerPaymentSettings unzerPaymentSettings,
            IStoreContext storeContext,
            IWorkContext workContext,
            ILogger logger,
            IWebHelper webHelper,
            IGenericAttributeService genericAttributeService,
            IOrderService orderService,
            IOrderProcessingService orderProcessingService,
            IUnzerHelperService unzerHelperService,
            ICustomerService customerService)
        {
            _unzerPaymentSettings = unzerPaymentSettings;
            _storeContext = storeContext;
            _workContext = workContext;
            _logger = logger;
            _webHelper = webHelper;
            _genericAttributeService = genericAttributeService;
            _orderService = orderService;
            _orderProcessingService = orderProcessingService;
            _unzerHelperService = unzerHelperService;
            _customerService = customerService;
        }

        #endregion

        #region Utilities

        private string GetPaymentTypePrefix(string resourceId)
        {
            var paymentTypePrefix = "";
            if (resourceId == null)
                return paymentTypePrefix;

            var resourceIdSplits = resourceId.Split('-');
            if (resourceIdSplits != null && resourceIdSplits.Length >= 2)
            {
                paymentTypePrefix = resourceIdSplits[1];
            }

            return paymentTypePrefix;
        }

        protected async Task AddNoteForAuthorizationAsync(Authorization authorization, Order order, string paymentId)
        {
            if (authorization.Status == Status.Success)
            {
                var msgStr = $"Unzer Authorized, Payment Id is '{paymentId}'.";

                var orderNote = new OrderNote()
                {
                    Note = msgStr,
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow,
                    OrderId = order.Id
                };
                await _orderService.InsertOrderNoteAsync(orderNote);

                // mark as authorized
                if (_orderProcessingService.CanMarkOrderAsAuthorized(order))
                {
                    await _orderProcessingService.MarkAsAuthorizedAsync(order);
                }
            }
            else
            {
                var errorStr = $"Unzer Authorization state is not Success. " +
                                $"Current State is '{authorization.Status}', Payment Id is '{paymentId}' and Card3ds is '{authorization.Card3ds}'.";

                var orderNote = new OrderNote()
                {
                    Note = errorStr,
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow,
                    OrderId = order.Id
                };
                await _orderService.InsertOrderNoteAsync(orderNote);
            }
        }

        protected async Task UpdateOrderPaymentStatusAndAddNoteAsync(Payment payment, Order order)
        {
            if (payment.State == State.Completed)
            {
                var msgStr = $"Unzer payment is completed, Payment Id is '{payment.Id}'. Amount Charged '{payment.AmountCharged}'";

                var orderNote = new OrderNote()
                {
                    Note = msgStr,
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow,
                    OrderId = order.Id
                };
                await _orderService.InsertOrderNoteAsync(orderNote);

                if (order.OrderTotal == payment.AmountCharged && _orderProcessingService.CanMarkOrderAsPaid(order))
                {
                    await _orderProcessingService.MarkOrderAsPaidAsync(order);

                    // remove redirectUrl from Order
                    order.SubscriptionTransactionId = null;

                    var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
                    // clear generic attribute value
                    await _genericAttributeService.SaveAttributeAsync<string>(customer, UnzerPaymentDefaults.PaymentIdAttribute, null, order.StoreId);
                }
            }
            else
            {
                var msgStr = $"Unzer payment State is {payment.State}, Payment Id is '{payment.Id}'.";

                var orderNote = new OrderNote()
                {
                    Note = msgStr,
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow,
                    OrderId = order.Id
                };
                await _orderService.InsertOrderNoteAsync(orderNote);
            }
        }

        protected async Task UpdateOrderPaymentStatusAndAddNoteForDirectChargeAsync(Payment payment, Order order, string uniqueId)
        {
            var charge = payment.ChargesList.FirstOrDefault(x => x.Processing.UniqueId.Equals(uniqueId));
            if (charge != null && charge.Status == Status.Success)
            {
                var msgStr = $"Unzer Charge Success. Payment Id is '{payment.Id}'. Charge Id is '{charge.Id}'";

                var orderNote = new OrderNote()
                {
                    Note = msgStr,
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow,
                    OrderId = order.Id
                };
                await _orderService.InsertOrderNoteAsync(orderNote);

                // keep the charge info as the Capture info
                order.CaptureTransactionId = charge.Id;
                order.CaptureTransactionResult = charge.Message?.Merchant;
            }
            else
            {
                var errorStr = $"Unzer Charge failed. Current Status is '{charge?.Status.ToString()}', Payment Id is '{payment.Id}'.";

                var orderNote = new OrderNote()
                {
                    Note = errorStr,
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow,
                    OrderId = order.Id
                };
                await _orderService.InsertOrderNoteAsync(orderNote);
            }
            await UpdateOrderPaymentStatusAndAddNoteAsync(payment, order);
        }

        #endregion

        #region Methods

        public async Task<IActionResult> CompleteChargeAndPayment()
        {
            var success = _webHelper.QueryString<bool>("success");
            var uuid = _webHelper.QueryString<string>("uuid");

            var paymentId = await _genericAttributeService.GetAttributeAsync<string>(await _workContext.GetCurrentCustomerAsync(), UnzerPaymentDefaults.PaymentIdAttribute, _storeContext.GetCurrentStore().Id);

            // add order note from query string
            var queryStrMsg = $"Unzer payment Id '{paymentId}' return with Auth/Charge success: '{success}' and unique process Id: '{uuid}'";

            if (paymentId != null)
            {
                var payment = await _unzerHelperService.FetchPaymentByPaymentIdAsync(paymentId);
                if (payment != null)
                {
                    var orderNumberGuid = Guid.Empty;
                    try
                    {
                        orderNumberGuid = new Guid(payment.OrderId);
                    }
                    catch
                    {
                        // ignored
                    }

                    var order = await _orderService.GetOrderByGuidAsync(orderNumberGuid);
                    if (order == null)
                    {
                        await _logger.ErrorAsync("In Unzer Can't retrieve the order." + queryStrMsg);
                        return RedirectToRoute("Homepage");
                    }

                    var orderNote1 = new OrderNote()
                    {
                        Note = queryStrMsg,
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow,
                        OrderId = order.Id
                    };
                    await _orderService.InsertOrderNoteAsync(orderNote1);

                    // validate with order authorization transaction code
                    if (paymentId != order.AuthorizationTransactionId)
                    {
                        var errorStr = $"Unzer payment Id '{paymentId}' does not match with order Authorization Transaction Code '{order.AuthorizationTransactionId}'. Order# '{order.Id}'.";
                        //log
                        await _logger.ErrorAsync(errorStr);

                        var orderNote = new OrderNote()
                        {
                            Note = errorStr,
                            DisplayToCustomer = false,
                            CreatedOnUtc = DateTime.UtcNow,
                            OrderId = order.Id
                        };
                        await _orderService.InsertOrderNoteAsync(orderNote);

                        return RedirectToRoute("Homepage");
                    }

                    //validate order total
                    if (payment.AmountTotal.HasValue && order.OrderTotal != payment.AmountTotal.Value)
                    {
                        var errorStr = $"Unzer payment order total {payment.AmountTotal.Value} doesn't equal order total '{order.OrderTotal}'. Order# {order.Id}.";
                        //log
                        await _logger.ErrorAsync(errorStr);

                        var orderNote = new OrderNote()
                        {
                            Note = errorStr,
                            DisplayToCustomer = false,
                            CreatedOnUtc = DateTime.UtcNow,
                            OrderId = order.Id
                        };
                        await _orderService.InsertOrderNoteAsync(orderNote);

                        return RedirectToRoute("Homepage");
                    }

                    // keeping resource Id here
                    var paymentTypePrefix = GetPaymentTypePrefix(order.AuthorizationTransactionCode);

                    if (paymentTypePrefix.Equals(UnzerPaymentDefaults.CardPaymentPrefix) || paymentTypePrefix.Equals(UnzerPaymentDefaults.PaypalPaymentPrefix))
                    {
                        // validate authorization
                        var authorization = payment.Authorization;
                        if (authorization == null)
                        {
                            var errorStr = $"Unzer Authorization can't be null. Payment Id '{payment.Id}', Payment state '{payment.State.ToString()}'. Order# {order.Id}.";

                            var orderNote = new OrderNote()
                            {
                                Note = errorStr,
                                DisplayToCustomer = false,
                                CreatedOnUtc = DateTime.UtcNow,
                                OrderId = order.Id
                            };
                            await _orderService.InsertOrderNoteAsync(orderNote);
                            return RedirectToRoute("Homepage");
                        }

                        // add authorization note
                        await AddNoteForAuthorizationAsync(authorization, order, paymentId);

                        if (payment.State == State.Pending && authorization.Status == Status.Success
                            && _unzerPaymentSettings.TransactionMode == TransactionMode.Charge)
                        {
                            // now create charge
                            var charge = _unzerHelperService.CreateChargeByAuthorizationAsync(authorization)?.Result;
                            if (charge != null && charge.Status == Status.Success)
                            {
                                var msgStr = $"Unzer Charge Success. Current Status is '{charge.Status}', Payment Id is '{payment.Id}'.";

                                var orderNote = new OrderNote()
                                {
                                    Note = msgStr,
                                    DisplayToCustomer = false,
                                    CreatedOnUtc = DateTime.UtcNow,
                                    OrderId = order.Id
                                };
                                await _orderService.InsertOrderNoteAsync(orderNote);

                                // keep the charge info as the Capture info
                                order.CaptureTransactionId = charge.Id;
                                order.CaptureTransactionResult = charge.Message?.Merchant;

                                // fetch payment again
                                var paymentAfterCharge = _unzerHelperService.FetchPaymentByPaymentIdAsync(paymentId)?.Result;
                                await UpdateOrderPaymentStatusAndAddNoteAsync(paymentAfterCharge, order);
                            }
                            else
                            {
                                var errorStr = $"Unzer Charge failed. Current Status is '{charge?.Status.ToString()}', Payment Id is '{payment.Id}'.";

                                var orderNote = new OrderNote()
                                {
                                    Note = errorStr,
                                    DisplayToCustomer = false,
                                    CreatedOnUtc = DateTime.UtcNow,
                                    OrderId = order.Id
                                };
                                await _orderService.InsertOrderNoteAsync(orderNote);

                                await UpdateOrderPaymentStatusAndAddNoteAsync(payment, order);
                            }
                        }
                        else
                        {
                            await UpdateOrderPaymentStatusAndAddNoteForDirectChargeAsync(payment, order, uuid);
                        }
                    }
                    else if (paymentTypePrefix.Equals(UnzerPaymentDefaults.SofortPaymentPrefix) || paymentTypePrefix.Equals(UnzerPaymentDefaults.EpsPaymentPrefix))
                    {
                        await UpdateOrderPaymentStatusAndAddNoteForDirectChargeAsync(payment, order, uuid);
                    }
                    else
                    {
                        var paymentTypeErrorStr = $"Unzer: Can't fetch payment type. Resource ID is invalid, Payment Id is '{payment.Id}'.";

                        var orderNote = new OrderNote()
                        {
                            Note = paymentTypeErrorStr,
                            DisplayToCustomer = false,
                            CreatedOnUtc = DateTime.UtcNow,
                            OrderId = order.Id
                        };
                        await _orderService.InsertOrderNoteAsync(orderNote);
                    }

                    await _orderService.UpdateOrderAsync(order);
                    return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });
                }

                await _logger.ErrorAsync($"In Unzer payment is NULL." + queryStrMsg);
                return RedirectToRoute("Homepage");
            }

            await _logger.ErrorAsync($"In Unzer Can't retrieve the paymentId" + queryStrMsg);
            return RedirectToRoute("Homepage");
        }

        #endregion
    }
}