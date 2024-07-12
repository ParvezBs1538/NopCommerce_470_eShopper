using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using NopStation.Plugin.Payments.Instamojo.Models;
using RestSharp;

namespace NopStation.Plugin.Payments.Instamojo.Services
{
    public class InstamojoManager
    {
        #region Fields

        private readonly IOrderService _orderService;
        private readonly ICurrencyService _currencyService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IWorkContext _workContext;
        private readonly IAddressService _addressService;
        private readonly IWebHelper _webHelper;
        private readonly ISettingService _settingService;
        private readonly ILogger _logger;

        #endregion

        #region Ctor

        public InstamojoManager(IOrderService orderService,
            ICurrencyService currencyService,
            IOrderProcessingService orderProcessingService,
            IWorkContext workContext,
            IAddressService addressService,
            IWebHelper webHelper,
            ISettingService settingService,
            ILogger logger)
        {
            _orderService = orderService;
            _currencyService = currencyService;
            _orderProcessingService = orderProcessingService;
            _workContext = workContext;
            _addressService = addressService;
            _webHelper = webHelper;
            _settingService = settingService;
            _logger = logger;
        }

        #endregion

        #region Utilities

        protected RestRequest GetRestRequest(InstamojoPaymentSettings instamojoPaymentSettings, Method method)
        {
            var request = new RestRequest(method);
            request.AddHeader("X-Api-Key", instamojoPaymentSettings.PrivateApiKey);
            request.AddHeader("X-Auth-Token", instamojoPaymentSettings.PrivateAuthToken);

            return request;
        }

        protected Uri GetbaseUrl(InstamojoPaymentSettings instamojoPaymentSettings)
        {
            if (instamojoPaymentSettings.UseSandbox)
                return new Uri("https://test.instamojo.com");

            return new Uri("https://www.instamojo.com");
        }

        protected string GetDecimalString(decimal value)
        {
            return value.ToString("#.##");
        }

        protected bool TryGetJson(object message, out IDictionary<string, string[]> errors)
        {
            errors = new Dictionary<string, string[]>();
            try
            {
                errors = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(message.ToString());
                return true;
            }
            catch
            {
                return false;
            }
        }

        protected string GetFormattedErrorMessage(object message)
        {
            if (TryGetJson(message, out var errors))
            {
                var sb = new StringBuilder();

                foreach (var error in errors)
                {
                    sb.AppendLine($"{error.Key} => ");
                    foreach (var item in error.Value)
                        sb.AppendLine(item);
                }

                return sb.ToString();
            }
            else
                return message.ToString();
        }

        #endregion

        #region Methods

        public async Task<string> GetPaymentUrlAsync(Order order)
        {
            try
            {
                var instamojoPaymentSettings = await _settingService.LoadSettingAsync<InstamojoPaymentSettings>(order.StoreId);

                var amount = _currencyService.ConvertCurrency(order.OrderTotal, order.CurrencyRate);
                var address = await _addressService.GetAddressByIdAsync(order.BillingAddressId);

                var url = GetbaseUrl(instamojoPaymentSettings).Concat("api/1.1/payment-requests/");
                var client = new RestClient(url);
                client.Timeout = -1;
                var request = GetRestRequest(instamojoPaymentSettings, Method.POST);
                request.AddHeader("content-type", "application/x-www-form-urlencoded");

                request.AddParameter("amount", GetDecimalString(amount));
                request.AddParameter("purpose", "Online purchase");
                request.AddParameter("buyer_name", $"{address.FirstName} {address.LastName}");
                request.AddParameter("email", address.Email);
                request.AddParameter("redirect_url", $"{_webHelper.GetStoreLocation()}instamojo/callback/{order.Id}");
                request.AddParameter("allow_repeated_payments", false);
                request.AddParameter("send_email", instamojoPaymentSettings.EnableSendEmail);
                request.AddParameter("send_sms", instamojoPaymentSettings.EnableSendSMS);

                if (string.IsNullOrWhiteSpace(instamojoPaymentSettings.PhoneNumberRegex) ||
                    new Regex(instamojoPaymentSettings.PhoneNumberRegex).IsMatch(address.PhoneNumber))
                    request.AddParameter("phone", address.PhoneNumber);

                var response = client.Execute(request);

                var model = JsonConvert.DeserializeObject<PaymentInitResponseModel>(response.Content);

                if (model.Success)
                {
                    order.AuthorizationTransactionId = model.PaymentRequest.Id;
                    await _orderService.UpdateOrderAsync(order);

                    return model.PaymentRequest.LongUrl;
                }
                else
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("Instamojo payment failed: ");
                    sb.AppendLine(GetFormattedErrorMessage(model.Message));

                    await _orderService.InsertOrderNoteAsync(new OrderNote()
                    {
                        CreatedOnUtc = DateTime.UtcNow,
                        Note = sb.ToString(),
                        OrderId = order.Id
                    });
                }

            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync(ex.Message, ex);
            }

            return null;
        }

        public async Task VerifyPaymentAsync(Order order)
        {
            try
            {
                var instamojoPaymentSettings = await _settingService.LoadSettingAsync<InstamojoPaymentSettings>(order.StoreId);

                var url = GetbaseUrl(instamojoPaymentSettings).Concat($"api/1.1/payment-requests/{order.AuthorizationTransactionId}");
                var client = new RestClient(url);

                client.Timeout = -1;
                var request = GetRestRequest(instamojoPaymentSettings, Method.GET);
                var response = client.Execute(request);

                var model = JsonConvert.DeserializeObject<PaymentDetailsModel>(response.Content);

                if (!model.Success)
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("Instamojo payment failed: ");
                    sb.AppendLine(GetFormattedErrorMessage(model.Message));

                    await _orderService.InsertOrderNoteAsync(new OrderNote()
                    {
                        CreatedOnUtc = DateTime.UtcNow,
                        Note = sb.ToString(),
                        OrderId = order.Id
                    });

                    return;
                }

                var amount = _currencyService.ConvertCurrency(order.OrderTotal, order.CurrencyRate);
                if (model.PaymentRequest.Amount >= amount)
                {
                    if (_orderProcessingService.CanMarkOrderAsPaid(order))
                    {
                        await _orderProcessingService.MarkOrderAsPaidAsync(order);

                        var sb = new StringBuilder();
                        sb.AppendLine("Order paid by Instamojo:");
                        sb.AppendLine($"payment_id: {model.PaymentRequest.Id}");
                        sb.AppendLine($"created_at: {model.PaymentRequest.CreatedAt}");

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
            catch (Exception ex)
            {
                await _logger.ErrorAsync(ex.Message, ex);
            }
        }

        public async Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest refundPaymentRequest)
        {
            var result = new RefundPaymentResult();
            try
            {
                var order = refundPaymentRequest.Order;

                var instamojoPaymentSettings = await _settingService.LoadSettingAsync<InstamojoPaymentSettings>(order.StoreId);

                var url = GetbaseUrl(instamojoPaymentSettings).Concat("/api/1.1/refunds/");
                var client = new RestClient(url);
                client.Timeout = -1;
                var request = GetRestRequest(instamojoPaymentSettings, Method.GET);
                request.AddHeader("content-type", "application/x-www-form-urlencoded");

                var amount = _currencyService.ConvertCurrency(refundPaymentRequest.AmountToRefund, order.CurrencyRate);
                request.AddParameter("refund_amount", GetDecimalString(amount));
                request.AddParameter("payment_id", order.AuthorizationTransactionId);
                request.AddParameter("type", "PTH");
                request.AddParameter("body", "Payment refunded");

                var response = client.Execute(request);

                var model = JsonConvert.DeserializeObject<RefundResponseModel>(response.Content);

                if (model.Success)
                {
                    await _orderService.InsertOrderNoteAsync(new OrderNote()
                    {
                        CreatedOnUtc = DateTime.UtcNow,
                        Note = $"{amount}{order.CustomerCurrencyCode} refunded (Receipt: {model.Refund.Id}) by {(await _workContext.GetCurrentCustomerAsync()).Email}",
                        OrderId = order.Id
                    });

                    result.NewPaymentStatus = refundPaymentRequest.IsPartialRefund ? PaymentStatus.PartiallyRefunded : PaymentStatus.Refunded;
                }
                else
                {
                    if (TryGetJson(model.Message, out var errors))
                    {
                        foreach (var error in errors)
                            foreach (var item in error.Value)
                                result.AddError(item);
                    }
                    else
                        result.AddError(model.Message.ToString());
                }
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync(ex.Message, ex);
                result.AddError(ex.Message);
            }

            return result;
        }

        #endregion
    }
}
