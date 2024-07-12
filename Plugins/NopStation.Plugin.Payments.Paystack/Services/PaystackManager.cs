using System;
using System.Linq;
using System.Text;
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
using NopStation.Plugin.Payments.Paystack.Models.Request;
using NopStation.Plugin.Payments.Paystack.Models.Response;
using RestSharp;

namespace NopStation.Plugin.Payments.Paystack.Services
{
    public class PaystackManager
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

        public PaystackManager(IOrderService orderService,
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

        protected RestRequest GetRestRequest(PaystackPaymentSettings paystackPaymentSettings, Method method)
        {
            var authHeaderValue = $"Bearer {paystackPaymentSettings.SecretKey}";

            var request = new RestRequest(method);
            request.AddHeader("Authorization", authHeaderValue);
            request.AddHeader("Content-Type", "application/json");

            return request;
        }

        #endregion

        #region Methods

        public async Task<string> GetPaymentLinkAsync(Order order)
        {
            try
            {
                var amount = _currencyService.ConvertCurrency(order.OrderTotal, order.CurrencyRate);
                var address = await _addressService.GetAddressByIdAsync(order.BillingAddressId);
                var referenceId = Guid.NewGuid().ToString().Replace("-", "");

                var paystackPaymentSettings = await _settingService.LoadSettingAsync<PaystackPaymentSettings>(order.StoreId);
                var client = new RestClient("https://api.paystack.co/transaction/initialize");
                client.Timeout = -1;
                var request = GetRestRequest(paystackPaymentSettings, Method.POST);

                var requestModel = new PaymentInitRequestModel()
                {
                    Amount = (int)(amount * 100),
                    CallbackUrl = $"{_webHelper.GetStoreLocation()}paystack/callback/{order.Id}",
                    Currency = order.CustomerCurrencyCode,
                    Email = address.Email,
                    Reference = referenceId
                };

                if (paystackPaymentSettings.Channels.Any())
                    requestModel.Channels = paystackPaymentSettings.Channels;

                var body = JsonConvert.SerializeObject(requestModel);
                request.AddParameter("application/json", body, ParameterType.RequestBody);
                var response = client.Execute(request);

                var responseModel = JsonConvert.DeserializeObject<PaymentInitResponseModel>(response.Content);

                if (!responseModel.Status)
                {
                    await _logger.ErrorAsync(responseModel.Message);
                    return null;
                }

                order.AuthorizationTransactionId = responseModel.Data.Reference;
                await _orderService.UpdateOrderAsync(order);
                return responseModel.Data.AuthorizationUrl;
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync(ex.Message, ex);
                return null;
            }
        }

        public async Task VerifyTransactionAsync(Order order)
        {
            try
            {
                var paystackPaymentSettings = await _settingService.LoadSettingAsync<PaystackPaymentSettings>(order.StoreId);
                var client = new RestClient($"https://api.paystack.co/transaction/verify/{order.AuthorizationTransactionId}");
                client.Timeout = -1;
                var request = GetRestRequest(paystackPaymentSettings, Method.GET);
                var response = client.Execute(request);

                var responseModel = JsonConvert.DeserializeObject<PaymentResponseModel>(response.Content);

                if (!responseModel.Status)
                {
                    await _logger.ErrorAsync($"Failed to pay by Paystack: {responseModel.Message}");
                    return;
                }

                var amount = _currencyService.ConvertCurrency(order.OrderTotal, order.CurrencyRate);
                if (responseModel.Data.Amount == amount * 100 && responseModel.Data.Currency.Equals(order.CustomerCurrencyCode, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (responseModel.Data.Status?.Equals("success", StringComparison.InvariantCultureIgnoreCase) ?? false && _orderProcessingService.CanMarkOrderAsPaid(order))
                    {
                        await _orderProcessingService.MarkOrderAsPaidAsync(order);

                        var sb = new StringBuilder();
                        sb.AppendLine("Order paid by Paystack:");
                        sb.AppendLine($"{JsonConvert.SerializeObject(responseModel, Formatting.Indented)}");

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
                var paystackPaymentSettings = await _settingService.LoadSettingAsync<PaystackPaymentSettings>(order.StoreId);

                var client = new RestClient($"https://api.paystack.co/refund");
                client.Timeout = -1;
                var request = GetRestRequest(paystackPaymentSettings, Method.GET);

                var amount = _currencyService.ConvertCurrency(refundPaymentRequest.AmountToRefund, order.CurrencyRate);
                var requestModel = new RefundRequestModel()
                {
                    Amount = (int)(amount * 100),
                    Currency = order.CustomerCurrencyCode,
                    Transaction = order.AuthorizationTransactionId
                };

                var body = JsonConvert.SerializeObject(requestModel);
                request.AddParameter("application/json", body, ParameterType.RequestBody);
                var response = client.Execute(request);

                var responseModel = JsonConvert.DeserializeObject<RefundResponseModel>(response.Content);

                if (responseModel.Status)
                {
                    await _orderService.InsertOrderNoteAsync(new OrderNote()
                    {
                        CreatedOnUtc = DateTime.UtcNow,
                        Note = $"{amount}{order.CustomerCurrencyCode} refunded (Transaction Id: {responseModel.Data.Transaction.Id}) by {(await _workContext.GetCurrentCustomerAsync()).Email}",
                        OrderId = order.Id
                    });

                    result.NewPaymentStatus = refundPaymentRequest.IsPartialRefund ? PaymentStatus.PartiallyRefunded : PaymentStatus.Refunded;
                }
                else
                    result.AddError(responseModel.Message);
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
