using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Data;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Stores;
using NopStation.Plugin.Payments.Quickstream.Models;

namespace NopStation.Plugin.Payments.Quickstream.Services
{
    public class QuickStreamPaymentService : IQuickStreamPaymentService
    {
        private readonly ISettingService _settingService;
        private readonly IOrderService _orderService;
        private readonly IRepository<Order> _orderRepository;
        private readonly ILogger _logger;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly QuickstreamSettings _quickstreamSettings;
        private readonly IWorkContext _workContext;
        private readonly IStoreService _storeService;

        public QuickStreamPaymentService(ISettingService settingService,
            IOrderService orderService,
            IRepository<Order> orderRepository,
            ILogger logger,
            IOrderProcessingService orderProcessingService,
            QuickstreamSettings quickstreamSettings,
            IWorkContext workContext,
            IStoreService storeService)
        {
            _settingService = settingService;
            _orderService = orderService;
            _orderRepository = orderRepository;
            _logger = logger;
            _orderProcessingService = orderProcessingService;
            _quickstreamSettings = quickstreamSettings;
            _workContext = workContext;
            _storeService = storeService;
        }

        public async Task<TakePaymentResponseBody> CompletePaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            var tokenId = processPaymentRequest.CustomValues["Token"];
            var takePaymentRequestBody = new TakePaymentRequestBody
            {
                TransactionType = QuickStreamDefaults.TRANSACTION_TYPE_PAYMENT,
                //TODO 
                Currency = (await _workContext.GetWorkingCurrencyAsync()).CurrencyCode,
                IpAddress = _quickstreamSettings.IpAddress,
                SingleUseTokenId = tokenId.ToString(),
                PrincipalAmount = Convert.ToDouble(processPaymentRequest.OrderTotal),
                Eci = QuickStreamDefaults.EC_INTERNET,
                SupplierBusinessCode = _quickstreamSettings.SupplierBusinessCode
            };

            var json = JsonConvert.SerializeObject(takePaymentRequestBody);

            var url = _quickstreamSettings.UseSandbox ? QuickStreamDefaults.SANDBOX_TAKE_PAYMENT_URL : QuickStreamDefaults.TAKE_PAYMENT_URL;
            var request = WebRequest.Create(url);
            request = QuickStreamPaymentHelper.AddHeaders(request, "POST", _quickstreamSettings.SecretApiKey);

            request.ContentLength = json.Length;
            using (var webStream = request.GetRequestStream())
            {
                using var requestWriter = new StreamWriter(webStream, Encoding.ASCII);
                requestWriter.Write(json);
            }

            var webResponse = request.GetResponse();
            using (var webStream = webResponse.GetResponseStream() ?? Stream.Null)
            {
                using var responseReader = new StreamReader(webStream);
                var response = await responseReader.ReadToEndAsync();
                var paymentUrlResponse = JsonConvert.DeserializeObject<TakePaymentResponseBody>(response);
                return paymentUrlResponse;
            }
        }

        public async Task<string> GetSingleUseTokenAsync(IFormCollection form)
        {
            var paymentType = "CREDIT_CARD";
            var creditCardSingleUseTokenRequestBody = new CreditCardSingleUseTokenRequestBody
            {
                CardholderName = form["CardholderName"],
                CardNumber = form["CardNumber"],
                ExpiryDateMonth = form["ExpireMonth"],
                ExpiryDateYear = form["ExpireYear"],
                Cvn = form["CardCode"],
                AccountType = paymentType,
                SupplierBusinessCode = _quickstreamSettings.SupplierBusinessCode
            };
            var json = JsonConvert.SerializeObject(creditCardSingleUseTokenRequestBody);

            var url = _quickstreamSettings.UseSandbox ? QuickStreamDefaults.SANDBOX_SINGLE_USE_TOKEN_URL : QuickStreamDefaults.SINGLE_USE_TOKEN_URL;
            var request = WebRequest.Create(url);
            request = paymentType == "CREDIT_CARD" ?
                QuickStreamPaymentHelper.AddHeaders(request, "POST", _quickstreamSettings.PublishableApiKey)
                : QuickStreamPaymentHelper.AddHeaders(request, "POST", _quickstreamSettings.PublishableApiKey);

            request.ContentLength = json.Length;
            using (var webStream = request.GetRequestStream())
            {
                using var requestWriter = new StreamWriter(webStream, Encoding.ASCII);
                requestWriter.Write(json);
            }

            var webResponse = request.GetResponse();
            using (var webStream = webResponse.GetResponseStream() ?? Stream.Null)
            {
                using var responseReader = new StreamReader(webStream);
                var response = await responseReader.ReadToEndAsync();
                var paymentUrlResponse = JsonConvert.DeserializeObject<SingleUseTokenResponse>(response);
                return paymentUrlResponse.SingleUseTokenId;
            }
        }

        public async Task<IList<Card>> GetAcceptCardsWithSurChargeAsync(int storeId)
        {
            var quickstreamSettings = await _settingService.LoadSettingAsync<QuickstreamSettings>(storeId);
            var url = quickstreamSettings.UseSandbox ? 
                string.Format(QuickStreamDefaults.SANDBOX_CARDS_SURCHARGE_URL, quickstreamSettings.SupplierBusinessCode)
                : string.Format(QuickStreamDefaults.CARDS_SURCHARGE_URL, quickstreamSettings.SupplierBusinessCode);

            var request = WebRequest.Create(url);
            request = QuickStreamPaymentHelper.AddHeadersWithoutContentType(request, "GET", quickstreamSettings.PublishableApiKey);

            var webResponse = request.GetResponse();
            using var webStream = webResponse.GetResponseStream() ?? Stream.Null;
            using var responseReader = new StreamReader(webStream);
            var response = await responseReader.ReadToEndAsync();
            var surChargeResponse = JsonConvert.DeserializeObject<SurChargeResponseBody>(response);
            return surChargeResponse.Cards;
        }

        public async Task<TakePaymentResponseBody> GetTransactionByReceiptNumberAsync(RefundPaymentRequest refundPaymentRequest)
        {
            var order = refundPaymentRequest.Order;

            var quickstreamSettings = await _settingService.LoadSettingAsync<QuickstreamSettings>(order.StoreId);
            var baseUrl = quickstreamSettings.UseSandbox ? 
                string.Format(QuickStreamDefaults.SANDBOX_GET_TRANSACTION_URL, order.CaptureTransactionId)
                : string.Format(QuickStreamDefaults.GET_TRANSACTION_URL, order.CaptureTransactionId);

            var request = WebRequest.Create(baseUrl);
            request = QuickStreamPaymentHelper.AddHeadersWithoutContentType(request, "GET", quickstreamSettings.SecretApiKey);

            var webResponse = request.GetResponse();
            using var webStream = webResponse.GetResponseStream() ?? Stream.Null;
            using var responseReader = new StreamReader(webStream);
            var response = responseReader.ReadToEnd();
            var paymentResponse = JsonConvert.DeserializeObject<TakePaymentResponseBody>(response);
            return paymentResponse;
        }

        public async Task<TakePaymentResponseBody> RefundAsync(RefundPaymentRequest refundPaymentRequest)
        {
            var order = refundPaymentRequest.Order;
            var refundRequestBody = new RefundRequestBody
            {
                PrincipalAmount = Convert.ToDouble(refundPaymentRequest.AmountToRefund),
                TransactionType = QuickStreamDefaults.TRANSACTION_TYPE_REFUND,
                OriginalReceiptNumber = order.CaptureTransactionId,
                CustomerReferenceNumber = order.CustomerId.ToString(),
                //TODO
                Currency = (await _workContext.GetWorkingCurrencyAsync()).CurrencyCode,
                Comment = "refund",
                PaymentReferenceNumber = $"refund order, orderId: {order.Id}"
            };

            var quickstreamSettings = await _settingService.LoadSettingAsync<QuickstreamSettings>(order.StoreId);
            var baseUrl = quickstreamSettings.UseSandbox ? QuickStreamDefaults.SANDBOX_REFUND_URL
                    : QuickStreamDefaults.REFUND_URL;

            var request = WebRequest.Create(baseUrl);

            request = QuickStreamPaymentHelper.AddHeaders(request, "POST", quickstreamSettings.SecretApiKey);
            var json = JsonConvert.SerializeObject(refundRequestBody);
            request.ContentLength = json.Length;
            using (var webStream = request.GetRequestStream())
            {
                using var requestWriter = new StreamWriter(webStream, Encoding.ASCII);
                requestWriter.Write(json);
            }

            var webResponse = request.GetResponse();
            using (var webStream = webResponse.GetResponseStream() ?? Stream.Null)
            {
                using var responseReader = new StreamReader(webStream);
                var response = responseReader.ReadToEnd();
                var paymentResponse = JsonConvert.DeserializeObject<TakePaymentResponseBody>(response);
                return paymentResponse;
            }
        }

        public async Task UpdateOrderPaymentStatusAsync()
        {
            foreach (var store in await _storeService.GetAllStoresAsync())
            {
                var query = _orderRepository.Table;
                var orders = query.Where(x =>
                        x.PaymentMethodSystemName == QuickStreamDefaults.PLUGIN_SYSTEM_NAME &&
                        x.PaymentStatusId == (int)PaymentStatus.Pending &&
                        x.CreatedOnUtc.AddDays(15) >= DateTime.UtcNow &&
                        x.CaptureTransactionResult != QuickStreamDefaults.APPROVED &&
                        x.StoreId == store.Id)
                    .ToList();

                var settings = await _settingService.LoadSettingAsync<QuickstreamSettings>(store.Id);

                foreach (var order in orders)
                {
                    var receiptNumber = order.CaptureTransactionId;

                    var baseUrl = settings.UseSandbox ? 
                        string.Format(QuickStreamDefaults.SANDBOX_GET_TRANSACTION_URL, receiptNumber)
                        : string.Format(QuickStreamDefaults.GET_TRANSACTION_URL, receiptNumber);

                    var request = WebRequest.Create(baseUrl);
                    request = QuickStreamPaymentHelper.AddHeadersWithoutContentType(request, "GET", settings.SecretApiKey);

                    try
                    {
                        var webResponse = request.GetResponse();
                        using var webStream = webResponse.GetResponseStream() ?? Stream.Null;
                        using var responseReader = new StreamReader(webStream);
                        var response = responseReader.ReadToEnd();
                        var paymentResponse = JsonConvert.DeserializeObject<TakePaymentResponseBody>(response);

                        if (paymentResponse.SummaryCode.Equals(QuickStreamDefaults.APPROVED))
                        {
                            await _orderProcessingService.MarkOrderAsPaidAsync(order);
                            //order.PaymentStatus = Core.Domain.Payments.PaymentStatus.Paid;
                            order.CaptureTransactionResult = paymentResponse.SummaryCode;
                            await _orderService.InsertOrderNoteAsync(new OrderNote
                            {
                                OrderId = order.Id,
                                Note = "Order payment completed.",
                                DisplayToCustomer = false,
                                CreatedOnUtc = DateTime.UtcNow
                            });
                            await _orderService.UpdateOrderAsync(order);
                        }
                        else if (order.CreatedOnUtc.AddDays(15) <= DateTime.UtcNow
                            && !paymentResponse.SummaryCode.Equals(QuickStreamDefaults.APPROVED))
                        {
                            await _orderService.InsertOrderNoteAsync(new OrderNote
                            {
                                OrderId = order.Id,
                                Note = "Payment declined or rejected",
                                DisplayToCustomer = true,
                                CreatedOnUtc = DateTime.UtcNow
                            });
                        }
                    }
                    catch (Exception e)
                    {
                        await _logger.ErrorAsync(e.Message);
                    }
                }
            }
        }
    }
}
