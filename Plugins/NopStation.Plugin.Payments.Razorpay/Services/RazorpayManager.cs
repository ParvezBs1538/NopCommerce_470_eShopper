using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using NopStation.Plugin.Payments.Razorpay.Models;
using NopStation.Plugin.Payments.Razorpay.Models.Request;
using NopStation.Plugin.Payments.Razorpay.Models.Response;
using RestSharp;

namespace NopStation.Plugin.Payments.Razorpay.Services
{
    public class RazorpayManager
    {
        #region Fields

        private readonly IOrderService _orderService;
        private readonly ILogger _logger;
        private readonly ICurrencyService _currencyService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IWorkContext _workContext;
        private readonly IAddressService _addressService;
        private readonly IWebHelper _webHelper;
        private readonly ISettingService _settingService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IPaymentService _paymentService;

        #endregion

        #region Ctor

        public RazorpayManager(IOrderService orderService,
            ILogger logger,
            ICurrencyService currencyService,
            IOrderProcessingService orderProcessingService,
            IWorkContext workContext,
            IAddressService addressService,
            IWebHelper webHelper,
            ISettingService settingService,
            IDateTimeHelper dateTimeHelper,
            IPaymentService paymentService)
        {
            _orderService = orderService;
            _logger = logger;
            _currencyService = currencyService;
            _orderProcessingService = orderProcessingService;
            _workContext = workContext;
            _addressService = addressService;
            _webHelper = webHelper;
            _settingService = settingService;
            _dateTimeHelper = dateTimeHelper;
            _paymentService = paymentService;
        }

        #endregion

        #region Utilities

        protected async Task<RestRequest> GetRestRequestAsync(int storeId, Method method)
        {
            var razorpayPaymentSettings = await _settingService.LoadSettingAsync<RazorpayPaymentSettings>(storeId);

            var basicAuthBytes = Encoding.GetEncoding("ISO-8859-1")
                .GetBytes($"{razorpayPaymentSettings.KeyId}:{razorpayPaymentSettings.KeySecret}");
            var authHeaderValue = $"Basic {Convert.ToBase64String(basicAuthBytes)}";

            var request = new RestRequest(method);
            request.AddHeader("Authorization", authHeaderValue);
            request.AddHeader("Content-Type", "application/json");

            return request;
        }

        #endregion

        #region Methods

        public async Task<string> GetPaymentLinkAsync(Order order)
        {
            var amount = _currencyService.ConvertCurrency(order.OrderTotal, order.CurrencyRate);
            var address = await _addressService.GetAddressByIdAsync(order.BillingAddressId);
            var referenceId = Guid.NewGuid().ToString();

            var client = new RestClient("https://api.razorpay.com/v1/payment_links");
            client.Timeout = -1;
            var request = await GetRestRequestAsync(order.StoreId, Method.POST);

            var requestModel = new CreatePaymentLinkRequestModel()
            {
                AcceptPartial = false,
                Amount = (int)(amount * 100),
                CallbackUrl = $"{_webHelper.GetStoreLocation()}razorpay/callback/{order.Id}",
                Currency = order.CustomerCurrencyCode,
                FirstMinPartialAmount = 0,
                CallbackMethod = "get",
                Customer = new CustomerModel()
                {
                    Email = address.Email,
                    Contact = address.PhoneNumber,
                    Name = $"{address.FirstName} {address.LastName}"
                },
                Description = $"Payment for order no #{order.CustomOrderNumber}",
                ExpireBy = (await _dateTimeHelper.ConvertToUserTimeAsync(DateTime.UtcNow, DateTimeKind.Utc)).AddMinutes(5).Ticks,
                ReferenceId = referenceId,
                ReminderEnable = false,
                Notes = new Dictionary<string, object>
                {
                    [RazorpayDefaults.OrderId] = order.Id,
                    [RazorpayDefaults.OrderGuid] = order.OrderGuid
                },
                Notify = new NotifyModel()
                {
                    Email = false,
                    Sms = false
                }
            };

            var body = JsonConvert.SerializeObject(requestModel);
            request.AddParameter("application/json", body, ParameterType.RequestBody);
            var response = client.Execute(request);

            var responseModel = JsonConvert.DeserializeObject<CreatePaymentLinkResponseModel>(response.Content);

            if (!string.IsNullOrWhiteSpace(responseModel.Error.Code))
                return null;

            var customAttributes = _paymentService.DeserializeCustomValues(order);
            if (customAttributes.ContainsKey(RazorpayDefaults.ReferenceId))
                customAttributes.Remove(RazorpayDefaults.ReferenceId);

            customAttributes.Add(RazorpayDefaults.ReferenceId, referenceId);
            var processPaymentRequest = new ProcessPaymentRequest();
            processPaymentRequest.CustomValues = customAttributes;
            var serializedXml = _paymentService.SerializeCustomValues(processPaymentRequest);

            order.CustomValuesXml = serializedXml;
            order.AuthorizationTransactionId = responseModel.Id;
            await _orderService.UpdateOrderAsync(order);

            return responseModel.ShortUrl;
        }

        public async Task VerifyPaymentAsync(Order order, IQueryCollection query)
        {
            var client = new RestClient($"https://api.razorpay.com/v1/payment_links/{order.AuthorizationTransactionId}");
            client.Timeout = -1;
            var request = await GetRestRequestAsync(order.StoreId, Method.GET);
            var response = client.Execute(request);

            var responseModel = JsonConvert.DeserializeObject<PaymentResponseModel>(response.Content);

            if (!string.IsNullOrWhiteSpace(responseModel.Error.Code))
            {
                await _logger.ErrorAsync(responseModel.Error.Description);
                return;
            }

            var amount = _currencyService.ConvertCurrency(order.OrderTotal, order.CurrencyRate);
            if (responseModel.Amount == (int)(amount * 100) && responseModel.Currency.Equals(order.CustomerCurrencyCode, StringComparison.InvariantCultureIgnoreCase))
            {
                if (responseModel.Status?.Equals("paid", StringComparison.InvariantCultureIgnoreCase) ?? false && _orderProcessingService.CanMarkOrderAsPaid(order))
                {
                    await _orderProcessingService.MarkOrderAsPaidAsync(order);

                    var paymentId = query["razorpay_payment_id"].ToString();
                    order.AuthorizationTransactionCode = paymentId;
                    await _orderService.UpdateOrderAsync(order);

                    var sb = new StringBuilder();
                    sb.AppendLine("Order paid by Razorpay:");
                    sb.AppendLine($"id: {responseModel.Id}");
                    sb.AppendLine($"razorpay_payment_id: {paymentId}");
                    sb.AppendLine($"razorpay_payment_link_reference_id: {query["razorpay_payment_link_reference_id"].ToString()}");
                    sb.AppendLine($"razorpay_payment_link_status: {query["razorpay_payment_link_status"].ToString()}");
                    sb.AppendLine($"razorpay_signature: {query["razorpay_signature"].ToString()}");
                    sb.AppendLine($"reference_id: {responseModel.ReferenceId}");
                    sb.AppendLine($"order_id: {responseModel.OrderId}");
                    sb.AppendLine($"user_id: {responseModel.UserId}");
                    sb.AppendLine($"currency: {responseModel.Currency}");
                    sb.AppendLine($"created_at: {responseModel.CreatedAt}");

                    var orderNote = new OrderNote()
                    {
                        CreatedOnUtc = DateTime.UtcNow,
                        DisplayToCustomer = false,
                        Note = sb.ToString(),
                        OrderId = order.Id
                    };
                    await _orderService.InsertOrderNoteAsync(orderNote);
                }
            }
        }

        public async Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest refundPaymentRequest)
        {
            var order = refundPaymentRequest.Order;

            var client = new RestClient($"https://api.razorpay.com/v1/payments/{order.AuthorizationTransactionCode}/refund");
            client.Timeout = -1;
            var request = await GetRestRequestAsync(order.StoreId, Method.POST);

            var receipt = Guid.NewGuid().ToString();
            var amount = _currencyService.ConvertCurrency(refundPaymentRequest.AmountToRefund, order.CurrencyRate);
            var requestModel = new RefundRequestModel()
            {
                Amount = (int)(amount * 100),
                Speed = "optimum",
                Receipt = receipt
            };

            var body = JsonConvert.SerializeObject(requestModel);
            request.AddParameter("application/json", body, ParameterType.RequestBody);
            var response = client.Execute(request);

            var responseModel = JsonConvert.DeserializeObject<RefundResponseModel>(response.Content);
            var result = new RefundPaymentResult();

            if (!string.IsNullOrWhiteSpace(responseModel.Error.Code))
            {
                result.AddError(responseModel.Error.Description);
                return result;
            }

            if (responseModel.Status?.Equals("processed", StringComparison.InvariantCultureIgnoreCase) ?? false)
            {
                await _orderService.InsertOrderNoteAsync(new OrderNote()
                {
                    CreatedOnUtc = DateTime.UtcNow,
                    Note = $"{amount}{order.CustomerCurrencyCode} refunded (Receipt: {receipt}) by {(await _workContext.GetCurrentCustomerAsync()).Email}",
                    OrderId = order.Id
                });

                result.NewPaymentStatus = refundPaymentRequest.IsPartialRefund ? PaymentStatus.PartiallyRefunded : PaymentStatus.Refunded;
            }
            else
                result.AddError("Failed to refund.");

            return result;
        }

        #endregion
    }
}
