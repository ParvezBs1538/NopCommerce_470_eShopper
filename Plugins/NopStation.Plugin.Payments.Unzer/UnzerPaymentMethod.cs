using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Heidelpay.Payment;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Nop.Core;
using Nop.Core.Domain.Cms;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;
using NopStation.Plugin.Misc.Core;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Payments.Unzer.Components;
using NopStation.Plugin.Payments.Unzer.Domain;
using NopStation.Plugin.Payments.Unzer.Models;
using NopStation.Plugin.Payments.Unzer.Services;

namespace NopStation.Plugin.Payments.Unzer
{
    public class UnzerPaymentMethod : BasePlugin, IPaymentMethod, INopStationPlugin, IAdminMenuPlugin, IWidgetPlugin
    {
        #region Fields

        private readonly ICurrencyService _currencyService;
        private readonly ICustomerService _customerService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly UnzerPaymentSettings _heidelpayPaymentSettings;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWorkContext _workContext;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IUnzerHelperService _unzerHelperService;
        private readonly IAddressService _addressService;
        private readonly INopStationCoreService _nopStationCoreService;
        private readonly IPermissionService _permissionService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly WidgetSettings _widgetSettings;

        #endregion

        #region Ctor

        public UnzerPaymentMethod(ICurrencyService currencyService,
            ICustomerService customerService,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            ILogger logger,
            ISettingService settingService,
            IWebHelper webHelper,
            UnzerPaymentSettings unzerPaymentSettings,
            IHttpContextAccessor httpContextAccessor,
            IWorkContext workContext,
            IOrderService orderService,
            IOrderProcessingService orderProcessingService,
            IUnzerHelperService unzerHelperService,
            IAddressService addressService,
            INopStationCoreService nopStationCoreService,
            IPermissionService permissionService,
            IOrderTotalCalculationService orderTotalCalculationService,
            WidgetSettings widgetSettings)
        {
            _currencyService = currencyService;
            _customerService = customerService;
            _genericAttributeService = genericAttributeService;
            _localizationService = localizationService;
            _logger = logger;
            _settingService = settingService;
            _webHelper = webHelper;
            _heidelpayPaymentSettings = unzerPaymentSettings;
            _httpContextAccessor = httpContextAccessor;
            _workContext = workContext;
            _orderService = orderService;
            _orderProcessingService = orderProcessingService;
            _unzerHelperService = unzerHelperService;
            _addressService = addressService;
            _nopStationCoreService = nopStationCoreService;
            _permissionService = permissionService;
            _orderTotalCalculationService = orderTotalCalculationService;
            _widgetSettings = widgetSettings;
        }

        #endregion

        #region Utilities

        private async Task<string> GetCustomerCurrencyCodeAsync(Nop.Core.Domain.Customers.Customer customer, int storeId)
        {
            var currencyTmp = await _currencyService.GetCurrencyByIdAsync(
                await _genericAttributeService.GetAttributeAsync<int>(customer, customer.CurrencyId.ToString(), storeId));
            var customerCurrency = currencyTmp != null && currencyTmp.Published ? currencyTmp : await _workContext.GetWorkingCurrencyAsync();

            return customerCurrency.CurrencyCode;
        }

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

        public async Task UpdateOrderPaymentStatusAndAddNoteAsync(Payment payment, Order order)
        {
            string msgStr;
            if (payment.State == State.Completed)
            {
                msgStr = $"Unzer payment is completed, Unzer Payment Id is {payment.Id}. Ammount Charged {payment.AmountCharged}";

                if (order.OrderTotal == payment.AmountCharged && _orderProcessingService.CanMarkOrderAsPaid(order))
                    await _orderProcessingService.MarkOrderAsPaidAsync(order);
            }
            else
                msgStr = $"Unzer payment State is {payment.State}, Unzer Payment Id is {payment.Id}.";

            var orderNote = new OrderNote()
            {
                Note = msgStr,
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow,
                OrderId = order.Id
            };
            await _orderService.InsertOrderNoteAsync(orderNote);
        }

        private async Task<ProcessPaymentResult> CardPaymentProcessAsync(ProcessPaymentRequest processPaymentRequest, string resourceId, string currencyCode, string unzerCustomerId)
        {
            //create Authorization
            var authorization = await _unzerHelperService.CreateAuthorizationForCardAsync(resourceId, processPaymentRequest.OrderGuid,
                    unzerCustomerId, processPaymentRequest.OrderTotal, currencyCode);

            if (authorization == null)
                throw new NopException("Card Payment Authorization Failed, Return null");

            //return result
            var result = new ProcessPaymentResult
            {
                // save authorization data
                AuthorizationTransactionId = authorization.PaymentId,
                // keeping resource Id here
                AuthorizationTransactionCode = resourceId
            };

            if (authorization.Status == Status.Success)
            {
                if (_heidelpayPaymentSettings.TransactionMode == TransactionMode.Charge)
                {
                    var charge = _unzerHelperService.CreateChargeByAuthorizationAsync(authorization)?.Result;

                    if (charge == null)
                        throw new NopException("Card Payment Charge Failed, Return null");

                    // keep the charge info as the Capture info
                    result.CaptureTransactionId = charge.Id;
                    result.CaptureTransactionResult = charge.Message?.Merchant;

                    if (charge.Status == Status.Success)
                    {
                        result.NewPaymentStatus = PaymentStatus.Paid;
                    }
                }
                else
                {
                    result.NewPaymentStatus = PaymentStatus.Authorized;
                }
            }
            else
            {
                result.NewPaymentStatus = PaymentStatus.Pending;
                // for providing CanRePostProcessPayment() payment approach, we are keeping redirectUrl here
                result.SubscriptionTransactionId = authorization.RedirectUrl?.OriginalString;
                // save PaymentId for further process
                await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(), UnzerPaymentDefaults.PaymentIdAttribute, authorization.PaymentId, processPaymentRequest.StoreId);
            }

            return result;
        }

        private async Task<ProcessPaymentResult> PaypalPaymentProcessAsync(ProcessPaymentRequest processPaymentRequest, string resourceId, string currencyCode, string unzerCustomerId)
        {
            //create Authorization
            var authorization = _unzerHelperService.CreatePaypalAuthorizationAsync(resourceId, processPaymentRequest.OrderGuid,
                    unzerCustomerId, processPaymentRequest.OrderTotal, currencyCode)?.Result;

            if (authorization == null)
                throw new NopException("PayPal Payment Authorization Failed, Return null");

            //return result
            var result = new ProcessPaymentResult
            {
                // save authorization data
                AuthorizationTransactionId = authorization.PaymentId,
                // keeping resource Id here
                AuthorizationTransactionCode = resourceId,
            };

            if (authorization.Status == Status.Success)
            {
                if (_heidelpayPaymentSettings.TransactionMode == TransactionMode.Charge)
                {
                    var charge = _unzerHelperService.CreateChargeByAuthorizationAsync(authorization)?.Result;

                    if (charge == null)
                        throw new NopException("Paypul Payment Charge Failed, Return null");

                    // keep the charge info as the Capture info
                    result.CaptureTransactionId = charge.Id;
                    result.CaptureTransactionResult = charge.Message?.Merchant;

                    if (charge.Status == Status.Success)
                    {
                        result.NewPaymentStatus = PaymentStatus.Paid;
                    }
                }
                else
                {
                    result.NewPaymentStatus = PaymentStatus.Authorized;
                }
            }
            else
            {
                result.NewPaymentStatus = PaymentStatus.Pending;
                // for providing CanRePostProcessPayment() payment approach, we are keeping redirectUrl here
                result.SubscriptionTransactionId = authorization.RedirectUrl?.OriginalString;
                // save PaymentId for further process
                await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(), UnzerPaymentDefaults.PaymentIdAttribute, authorization.PaymentId, processPaymentRequest.StoreId);
            }

            return result;
        }

        private async Task<ProcessPaymentResult> SofortPaymentProcessAsync(ProcessPaymentRequest processPaymentRequest, string resourceId, string currencyCode, string unzerCustomerId)
        {
            //create Charge
            var charge = _unzerHelperService.CreateChargeForSofortAsync(resourceId, processPaymentRequest.OrderGuid,
                    unzerCustomerId, processPaymentRequest.OrderTotal, currencyCode)?.Result;

            if (charge == null)
                throw new NopException("Sofort Payment Charge Failed, Return null");

            //return result
            var result = new ProcessPaymentResult
            {
                // save authorization data
                AuthorizationTransactionId = charge.PaymentId,
                // keeping resource Id here
                AuthorizationTransactionCode = resourceId
            };

            // keep the charge info as the Capture info
            result.CaptureTransactionId = charge.Id;
            result.CaptureTransactionResult = charge.Message?.Merchant;

            if (charge.Status == Status.Success)
            {
                result.NewPaymentStatus = PaymentStatus.Paid;
            }
            else
            {
                result.NewPaymentStatus = PaymentStatus.Pending;
                // for providing CanRePostProcessPayment() payment approach, we are keeping redirectUrl here
                result.SubscriptionTransactionId = charge.RedirectUrl?.OriginalString;
                // save PaymentId for further process
                await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(), UnzerPaymentDefaults.PaymentIdAttribute, charge.PaymentId, processPaymentRequest.StoreId);
            }

            return result;
        }

        private async Task<ProcessPaymentResult> EpsPaymentProcessAsync(ProcessPaymentRequest processPaymentRequest, string resourceId, string currencyCode, string epsBic, string unzerCustomerId)
        {
            //create Charge
            var charge = _unzerHelperService.CreateChargeForEPSAsync(resourceId, processPaymentRequest.OrderGuid,
                    unzerCustomerId, processPaymentRequest.OrderTotal, currencyCode, epsBic)?.Result;

            if (charge == null)
                throw new NopException("EPS Payment Charge Failed, Return null");

            //return result
            var result = new ProcessPaymentResult
            {
                // save authorization data
                AuthorizationTransactionId = charge.PaymentId,
                // keeping resource Id here
                AuthorizationTransactionCode = resourceId
            };

            // keep the charge info as the Capture info
            result.CaptureTransactionId = charge.Id;
            result.CaptureTransactionResult = charge.Message?.Merchant;

            if (charge.Status == Status.Success)
            {
                result.NewPaymentStatus = PaymentStatus.Paid;
            }
            else
            {
                result.NewPaymentStatus = PaymentStatus.Pending;
                // for providing CanRePostProcessPayment() payment approach, we are keeping redirectUrl here
                result.SubscriptionTransactionId = charge.RedirectUrl?.OriginalString;
                // save PaymentId for further process
                await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(), UnzerPaymentDefaults.PaymentIdAttribute, charge.PaymentId, processPaymentRequest.StoreId);
            }

            return result;
        }

        #endregion

        #region Methods

        public async Task<string> GetPaymentMethodDescriptionAsync()
        {
            return await _localizationService.GetResourceAsync("NopStation.Unzer.PaymentMethodDescription");
        }

        public async Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            if (processPaymentRequest == null)
                throw new ArgumentException(nameof(processPaymentRequest));

            //try to get the ResourceID
            var resourceIDKey = UnzerPaymentDefaults.PaymentResourceIDKey;
            if (!processPaymentRequest.CustomValues.TryGetValue(resourceIDKey, out var resourceId) || string.IsNullOrEmpty(resourceId?.ToString()))
                throw new NopException("Failed to get the Resource ID");

            var customer = await _customerService.GetCustomerByIdAsync(processPaymentRequest.CustomerId);
            if (customer == null)
                throw new NopException("Customer is not found");

            var currencyCode = await GetCustomerCurrencyCodeAsync(customer, processPaymentRequest.StoreId);
            if (string.IsNullOrEmpty(currencyCode))
                throw new NopException("Currency cannot be loaded");

            // unzer customer
            var unzerCustomerId = await _genericAttributeService.GetAttributeAsync<string>(customer, UnzerPaymentDefaults.CustomerIdAttribute, processPaymentRequest.StoreId);

            var billingAddress = await _addressService.GetAddressByIdAsync(customer.BillingAddressId ?? 0);
            var firstName = billingAddress?.FirstName;
            var lastName = billingAddress?.LastName;
            var email = billingAddress?.Email;

            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
                throw new NopException("First name or Last name is missing in billing address");

            if (string.IsNullOrEmpty(email))
                throw new NopException("Email is missing in billing address");

            if (string.IsNullOrEmpty(unzerCustomerId))
            {
                unzerCustomerId = _unzerHelperService.CreateUnzerCustomerAsync(firstName, lastName, email)?.Result;

                // save Unzer Customer Id
                await _genericAttributeService.SaveAttributeAsync(customer, UnzerPaymentDefaults.CustomerIdAttribute, unzerCustomerId, processPaymentRequest.StoreId);
            }
            else
            {
                unzerCustomerId = await _unzerHelperService.GetUnzerCustomerAsync(unzerCustomerId);
                if (string.IsNullOrEmpty(unzerCustomerId))
                    unzerCustomerId = _unzerHelperService.UpdateUnzerCustomerAsync(unzerCustomerId, firstName, lastName, email)?.Result;

                // save Unzer Customer Id
                await _genericAttributeService.SaveAttributeAsync(customer, UnzerPaymentDefaults.CustomerIdAttribute, unzerCustomerId, processPaymentRequest.StoreId);
            }

            if (string.IsNullOrEmpty(unzerCustomerId))
                throw new NopException("Unzer Customer create or get failed");

            var paymentTypePrefix = GetPaymentTypePrefix(resourceId?.ToString());

            if (paymentTypePrefix.Equals(UnzerPaymentDefaults.CardPaymentPrefix))
            {
                return await CardPaymentProcessAsync(processPaymentRequest, resourceId.ToString(), currencyCode, unzerCustomerId);
            }
            else if (paymentTypePrefix.Equals(UnzerPaymentDefaults.PaypalPaymentPrefix))
            {
                return await PaypalPaymentProcessAsync(processPaymentRequest, resourceId.ToString(), currencyCode, unzerCustomerId);
            }
            else if (paymentTypePrefix.Equals(UnzerPaymentDefaults.SofortPaymentPrefix))
            {
                return await SofortPaymentProcessAsync(processPaymentRequest, resourceId.ToString(), currencyCode, unzerCustomerId);
            }
            else if (paymentTypePrefix.Equals(UnzerPaymentDefaults.EpsPaymentPrefix))
            {
                var epsBicKey = await _localizationService.GetResourceAsync("NopStation.Unzer.EPSBICKey");
                if (!processPaymentRequest.CustomValues.TryGetValue(epsBicKey, out var epsBic) || string.IsNullOrEmpty(epsBic?.ToString()))
                    throw new NopException("Failed to get the EPS BIC");

                return await EpsPaymentProcessAsync(processPaymentRequest, resourceId.ToString(), currencyCode, epsBic.ToString(), unzerCustomerId);
            }

            throw new NopException("Resource ID is invalid");
        }

        public async Task PostProcessPaymentAsync(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            var order = postProcessPaymentRequest.Order;
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);
            await _genericAttributeService.SaveAttributeAsync(customer, UnzerPaymentDefaults.PaymentIdAttribute, order.AuthorizationTransactionId, order.StoreId);

            var url = order.SubscriptionTransactionId;
            if (url != null)
                _httpContextAccessor.HttpContext.Response.Redirect(url);
        }

        public Task<bool> HidePaymentMethodAsync(IList<ShoppingCartItem> cart)
        {
            return Task.FromResult(false);
        }

        public async Task<decimal> GetAdditionalHandlingFeeAsync(IList<ShoppingCartItem> cart)
        {
            return await _orderTotalCalculationService.CalculatePaymentAdditionalFeeAsync(cart,
                _heidelpayPaymentSettings.AdditionalFee, _heidelpayPaymentSettings.AdditionalFeePercentage);
        }

        public async Task<CapturePaymentResult> CaptureAsync(CapturePaymentRequest capturePaymentRequest)
        {
            if (capturePaymentRequest == null)
                throw new ArgumentException(nameof(capturePaymentRequest));

            var result = new CapturePaymentResult();

            //capture transaction
            var paymentId = capturePaymentRequest.Order.AuthorizationTransactionId;

            if (paymentId != null)
            {
                var payment = _unzerHelperService.FetchPaymentByPaymentIdAsync(paymentId)?.Result;
                var order = capturePaymentRequest.Order;

                if (payment != null)
                {
                    //validate order total
                    if (payment.AmountTotal.HasValue && order.OrderTotal != payment.AmountTotal.Value)
                    {
                        var errorStr = $"Unzer Returned order total '{payment.AmountTotal.Value}' doesn't equal order total '{order.OrderTotal}'. Order# {order.Id}.";

                        var orderNote = new OrderNote()
                        {
                            Note = errorStr,
                            DisplayToCustomer = false,
                            CreatedOnUtc = DateTime.UtcNow,
                            OrderId = order.Id
                        };
                        await _orderService.InsertOrderNoteAsync(orderNote);

                        result.AddError(errorStr);
                        return result;
                    }

                    // validate authorization
                    var authorization = payment.Authorization;
                    if (authorization == null)
                    {
                        var errorStr = $"Unzer Authorization can't be null. Payment Id '{payment.Id}', payment state '{payment.State.ToString()}'. Order# {order.Id}.";
                        var orderNote = new OrderNote()
                        {
                            Note = errorStr,
                            DisplayToCustomer = false,
                            CreatedOnUtc = DateTime.UtcNow,
                            OrderId = order.Id
                        };
                        await _orderService.InsertOrderNoteAsync(orderNote);

                        result.AddError(errorStr);
                        return result;
                    }

                    if (payment.State == State.Pending && authorization.Status == Status.Success)
                    {
                        // now create charge
                        var charge = _unzerHelperService.CreateChargeByAuthorizationAsync(authorization)?.Result;
                        if (charge != null && charge.Status == Status.Success)
                        {
                            var msgStr = $"Unzer Charge Success. Payment Id is '{payment.Id}', Charge Id is '{charge.Id}'";
                            var orderNote = new OrderNote()
                            {
                                Note = msgStr,
                                DisplayToCustomer = false,
                                CreatedOnUtc = DateTime.UtcNow,
                                OrderId = order.Id
                            };
                            await _orderService.InsertOrderNoteAsync(orderNote);

                            // fetch payment again
                            var paymentAfterCharge = await _unzerHelperService.FetchPaymentByPaymentIdAsync(paymentId);
                            await UpdateOrderPaymentStatusAndAddNoteAsync(paymentAfterCharge, order);
                            await _orderService.UpdateOrderAsync(order);

                            //successfully captured
                            result.NewPaymentStatus = PaymentStatus.Paid;
                            // keep the charge id as the Capture Transaction Id
                            result.CaptureTransactionId = charge.Id;
                            result.CaptureTransactionResult = charge.Message?.Merchant;

                            return result;
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

                            result.AddError(errorStr);
                            return result;
                        }
                    }

                    var captureErrorStr = $"Unzer Payment capture failed. Current authorization Status is '{authorization.Status}' " +
                        $"and payment State is '{payment.State}'";
                    result.AddError(captureErrorStr);
                    return result;
                }
            }

            var paymentErrorStr = "Unzer Payment is not found with this transcactionId.";
            result.AddError(paymentErrorStr);
            return result;
        }

        public async Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest refundPaymentRequest)
        {
            if (refundPaymentRequest == null)
                throw new ArgumentException(nameof(refundPaymentRequest));

            var order = refundPaymentRequest.Order;
            var paymentId = order.AuthorizationTransactionId;

            var result = new RefundPaymentResult();

            if (paymentId != null)
            {
                var payment = _unzerHelperService.FetchPaymentByPaymentIdAsync(paymentId)?.Result;

                if (payment != null)
                {
                    var chargeId = order.CaptureTransactionId;

                    if (chargeId != null)
                    {
                        var charge = _unzerHelperService.FetchChargeByPaymentIdAndChargeIdAsync(paymentId, chargeId);

                        if (charge != null)
                        {
                            Cancel cancel;
                            if (refundPaymentRequest.IsPartialRefund)
                                cancel = _unzerHelperService.CancelChargeByPaymentIdAndChargeIdAsync(paymentId, chargeId, refundPaymentRequest.AmountToRefund).Result;
                            else
                                cancel = _unzerHelperService.CancelChargeByPaymentIdAndChargeIdAsync(paymentId, chargeId)?.Result;

                            if (cancel != null)
                            {
                                if (cancel.Status == Status.Success)
                                {
                                    var orderNote = new OrderNote()
                                    {
                                        Note = $"Unzer charge succefully cancelled.Canceled Ammount is '{cancel.Amount}'. Here charge Id '{chargeId}' and Payment Id '{paymentId}'",
                                        DisplayToCustomer = false,
                                        CreatedOnUtc = DateTime.UtcNow,
                                        OrderId = order.Id
                                    };
                                    await _orderService.InsertOrderNoteAsync(orderNote);
                                    result.NewPaymentStatus = refundPaymentRequest.IsPartialRefund ? PaymentStatus.PartiallyRefunded : PaymentStatus.Refunded;
                                    return result;
                                }

                                var cancelStatusStr = $"Unzer charge cancel is not success. Cancel Status  is '{cancel.Status.ToString()}'. Here charge Id '{chargeId}' and Payment Id '{paymentId}'";
                                result.AddError(cancelStatusStr);
                                return result;
                            }

                            var cancelErrorStr = $"Unzer charge cancel failed with this charge Id '{chargeId}' and Payment Id '{paymentId}'";
                            result.AddError(cancelErrorStr);
                            return result;
                        }
                    }

                    var chargeErrorStr = $"Unzer charge is not found with this charge Id '{chargeId}'";
                    result.AddError(chargeErrorStr);
                    return result;
                }
            }

            var paymentErrorStr = $"Unzer Payment is not found with this transcactionId / payment Id '{paymentId}'";
            result.AddError(paymentErrorStr);
            return result;
        }

        public async Task<VoidPaymentResult> VoidAsync(VoidPaymentRequest voidPaymentRequest)
        {
            if (voidPaymentRequest == null)
                throw new ArgumentException(nameof(voidPaymentRequest));

            var order = voidPaymentRequest.Order;
            var paymentId = order.AuthorizationTransactionId;

            var result = new VoidPaymentResult();

            if (paymentId != null)
            {
                var payment = _unzerHelperService.FetchPaymentByPaymentIdAsync(paymentId)?.Result;

                if (payment != null)
                {
                    var cancel = _unzerHelperService.CancelAuthorizationByPaymentIdAsync(paymentId)?.Result;

                    if (cancel != null)
                    {
                        if (cancel.Status == Status.Success)
                        {
                            var orderNote = new OrderNote()
                            {
                                Note = $"Unzer Authorization succefully cancelled. Canceled Ammount is {cancel.Amount}. Here Payment Id {paymentId}",
                                DisplayToCustomer = false,
                                CreatedOnUtc = DateTime.UtcNow,
                                OrderId = order.Id
                            };
                            await _orderService.InsertOrderNoteAsync(orderNote);
                            result.NewPaymentStatus = PaymentStatus.Voided;
                            return result;
                        }

                        var cancelStatusStr = $"Unzer Authorization cancel is not success. Cancel Status  is {cancel.Status.ToString()}. Here Payment Id {paymentId}";
                        result.AddError(cancelStatusStr);
                        return result;
                    }

                    var cancelErrorStr = $"Unzer Authorization cancel failed with this Payment Id {paymentId}";
                    result.AddError(cancelErrorStr);
                    return result;
                }
            }

            var paymentErrorStr = $"Unzer Payment is not found with this transcactionId / payment Id {paymentId}";
            result.AddError(paymentErrorStr);
            return result;
        }

        public Task<ProcessPaymentResult> ProcessRecurringPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            throw new ArgumentException(nameof(processPaymentRequest));
        }

        public Task<CancelRecurringPaymentResult> CancelRecurringPaymentAsync(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            if (cancelPaymentRequest == null)
                throw new ArgumentException(nameof(cancelPaymentRequest));

            //always success
            return Task.FromResult(new CancelRecurringPaymentResult());
        }

        public Task<bool> CanRePostProcessPaymentAsync(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            if ((DateTime.UtcNow - order.CreatedOnUtc).TotalSeconds < 5)
                return Task.FromResult(false);

            if (string.IsNullOrEmpty(order.SubscriptionTransactionId))
                return Task.FromResult(false);

            return Task.FromResult(true);
        }

        public Task<IList<string>> ValidatePaymentFormAsync(IFormCollection form)
        {
            if (form.TryGetValue(nameof(PaymentInfoModel.Errors), out var errorsString) && !StringValues.IsNullOrEmpty(errorsString))
                return Task.FromResult<IList<string>>(errorsString.ToString().Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries).ToList());

            return Task.FromResult<IList<string>>(new List<string>());
        }

        public async Task<ProcessPaymentRequest> GetPaymentInfoAsync(IFormCollection form)
        {
            var paymentRequest = new ProcessPaymentRequest();

            if (form.TryGetValue(nameof(PaymentInfoModel.ResourceID), out var resourceId) && !StringValues.IsNullOrEmpty(resourceId))
                paymentRequest.CustomValues.Add(UnzerPaymentDefaults.PaymentResourceIDKey, resourceId.ToString());

            if (!StringValues.IsNullOrEmpty(resourceId))
            {
                var paymentTypePrefix = GetPaymentTypePrefix(resourceId.ToString());
                if (paymentTypePrefix.Equals(UnzerPaymentDefaults.EpsPaymentPrefix))
                {
                    if (form.TryGetValue(nameof(PaymentInfoModel.EPSBIC), out var epsBic) && !StringValues.IsNullOrEmpty(epsBic))
                        paymentRequest.CustomValues.Add(await _localizationService.GetResourceAsync("NopStation.Unzer.EPSBICKey"), epsBic.ToString());
                }
            }

            return paymentRequest;
        }

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/Unzer/Configure";
        }

        public override async Task InstallAsync()
        {
            //settings
            await _settingService.SaveSettingAsync(new UnzerPaymentSettings
            {
                TransactionMode = TransactionMode.Charge,
                ApiEndpoint = "https://api.heidelpay.com",
                ApiVersion = "v1",
                UseSandbox = true,
                IsCardActive = true,
                IsPaypalActive = true,
                IsSofortActive = true,
                IsEpsActive = true
            });

            await this.InstallPluginAsync(new UnzerPermissionProvider());

            if (!_widgetSettings.ActiveWidgetSystemNames.Contains(UnzerPaymentDefaults.SystemName))
            {
                _widgetSettings.ActiveWidgetSystemNames.Add(UnzerPaymentDefaults.SystemName);
                await _settingService.SaveSettingAsync(_widgetSettings);
            }

            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            //settings
            await _settingService.DeleteSettingAsync<UnzerPaymentSettings>();
            await this.UninstallPluginAsync(new UnzerPermissionProvider());
            await base.UninstallAsync();
        }

        public List<KeyValuePair<string, string>> PluginResouces()
        {
            var list = new List<KeyValuePair<string, string>>();

            list.Add(new KeyValuePair<string, string>("NopStation.Unzer.PaymentMethodDescription", "All-in-one Payment (Mastercard, Visa, PayPal, SOFORT, EPS...)"));
            list.Add(new KeyValuePair<string, string>("NopStation.Unzer.EPSBICKey", "Provided Eps Bic"));

            list.Add(new KeyValuePair<string, string>("Enums.NopStation.Plugin.Payments.Unzer.Domain.TransactionMode.Authorize", "Authorize only"));
            list.Add(new KeyValuePair<string, string>("Enums.NopStation.Plugin.Payments.Unzer.Domain.TransactionMode.Charge", "Charge (authorize and capture)"));

            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Unzer.Configuration.Fields.AdditionalFee", "Additional fee"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Unzer.Configuration.Fields.AdditionalFee.Hint", "Enter additional fee to charge your customers."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Unzer.Configuration.Fields.PrivateKey", "Private key"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Unzer.Configuration.Fields.PrivateKey.Hint", "Enter your private key."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Unzer.Configuration.Fields.PublicKey", "Public key"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Unzer.Configuration.Fields.PublicKey.Hint", "Enter your public key."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Unzer.Configuration.Fields.AdditionalFeePercentage", "Additional fee. Use percentage"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Unzer.Configuration.Fields.AdditionalFeePercentage.Hint", "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Unzer.Configuration.Fields.UseSandbox", "Use sandbox"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Unzer.Configuration.Fields.UseSandbox.Hint", "Determine whether to use sandbox credentials."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Unzer.Configuration.Fields.TransactionMode", "Transaction mode"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Unzer.Configuration.Fields.TransactionMode.Hint", "Choose the transaction mode."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Unzer.Configuration.Fields.Endpoint.Hint", "Enter api endpoint."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Unzer.Configuration.Fields.Endpoint", "Endpoint"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Unzer.Configuration.Fields.ApiVersion.Hint", "Enter api version."));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Unzer.Configuration.Fields.ApiVersion", "Api version"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Unzer.Configuration", "Unzer settings"));

            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Unzer.Configuration.Fields.IsCardActive", "Is card payment active"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Unzer.Configuration.Fields.IsCardActive.Hint", "Indicating whether to card payment is active or not active"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Unzer.Configuration.Fields.IsPaypalActive", "Is PayPal payment active"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Unzer.Configuration.Fields.IsPaypalActive.Hint", "Indicating whether to PayPal payment is active or not active"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Unzer.Configuration.Fields.IsSofortActive", "Is Sofort payment Active"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Unzer.Configuration.Fields.IsSofortActive.Hint", "Indicating whether to Sofort payment is active or not active"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Unzer.Configuration.Fields.IsEpsActive", "Is EPS payment Active"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Unzer.Configuration.Fields.IsEpsActive.Hint", "Indicating whether to EPS payment is active or not active"));

            list.Add(new KeyValuePair<string, string>("NopStation.Unzer.Info.NoPaymentTypeActive", "No payment type is active now for HeidelPay, Please select another Payment Method"));

            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Unzer.Menu.Unzer", "Unzer payment"));
            list.Add(new KeyValuePair<string, string>("Admin.NopStation.Unzer.Menu.Configuration", "Configuration"));

            return list;
        }

        public async Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                Title = await _localizationService.GetResourceAsync("Admin.NopStation.Unzer.Menu.Unzer"),
                Visible = true,
                IconClass = "far fa-dot-circle",
            };

            if (await _permissionService.AuthorizeAsync(UnzerPermissionProvider.ManageConfiguration))
            {
                var categoryIconItem = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Unzer.Menu.Configuration"),
                    Url = "~/Admin/Unzer/Configure",
                    Visible = true,
                    IconClass = "far fa-circle",
                    SystemName = "Unzer.Configuration"
                };
                menuItem.ChildNodes.Add(categoryIconItem);
            }
            if (await _permissionService.AuthorizeAsync(CorePermissionProvider.ShowDocumentations))
            {
                var documentation = new SiteMapNode()
                {
                    Title = await _localizationService.GetResourceAsync("Admin.NopStation.Common.Menu.Documentation"),
                    Url = "https://www.nop-station.com/unzer-documentation?utm_source=admin-panel&utm_medium=products&utm_campaign=unzer",
                    Visible = true,
                    IconClass = "far fa-circle",
                    OpenUrlInNewTab = true
                };
                menuItem.ChildNodes.Add(documentation);
            }

            await _nopStationCoreService.ManageSiteMapAsync(rootNode, menuItem, NopStationMenuType.Plugin);
        }

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string>
            {
                PublicWidgetZones.CheckoutPaymentInfoTop,
                PublicWidgetZones.OpcContentBefore
            });
        }

        public Type GetPublicViewComponent()
        {
            return typeof(UnzerViewComponent);
        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            if (widgetZone == null)
                return null;
            if (widgetZone.Equals(PublicWidgetZones.CheckoutPaymentInfoTop) ||
                widgetZone.Equals(PublicWidgetZones.OpcContentBefore))
            {
                return typeof(UnzerScript);
            }
            return default;
        }


        #endregion

        #region Properties

        public bool SupportCapture => true;

        public bool SupportPartiallyRefund => true;

        public bool SupportRefund => true;

        public bool SupportVoid => true;

        public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.Manual;

        public PaymentMethodType PaymentMethodType => PaymentMethodType.Redirection;

        public bool SkipPaymentInfo => false;

        public bool HideInWidgetList => false;

        #endregion
    }
}