using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Services.Directory;
using Nop.Services.Logging;
using Nop.Services.Orders;
using NopStation.Plugin.Misc.Core.Controllers;
using NopStation.Plugin.Payments.POLiPay;
using NopStation.Plugin.Payments.POLiPay.Models;

namespace NopStation.Plugin.Payments.POLiPayment.Controllers
{
    public class PaymentPolipayController : NopStationPublicController
    {
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderService _orderService;
        private readonly ILogger _logger;
        private readonly IWebHelper _webHelper;
        private readonly ICurrencyService _currencyService;
        private readonly IWorkContext _workContext;
        private readonly PoliPaySettings _poliPaySettings;

        public PaymentPolipayController(IOrderProcessingService orderProcessingService,
            IOrderService orderService,
            IWebHelper webHelper,
            ICurrencyService currencyService,
            IWorkContext workContext,
            PoliPaySettings poliPaySettings,
            ILogger logger)
        {
            _orderProcessingService = orderProcessingService;
            _orderService = orderService;
            _webHelper = webHelper;
            _currencyService = currencyService;
            _workContext = workContext;
            _poliPaySettings = poliPaySettings;
            _logger = logger;
        }

        public async Task<IActionResult> PostPaymentHandler(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                await _logger.ErrorAsync("Invalid token. Redirecting to homepage.");
                return RedirectToRoute("Homepage");
            }

            var baseUrl = _poliPaySettings.UseSandbox ? PoliPayDefaults.SANDBOX_BASE_URL : PoliPayDefaults.BASE_URL;
            var resourceAddress = string.Format(PoliPayDefaults.GET_STATUS_RESOURCE, token);

            var request = WebRequest.Create($"{baseUrl}{resourceAddress}");
            request = PoliPayHelper.AddHeaders(request, "GET", _poliPaySettings);
            try
            {
                var webResponse = request.GetResponse();
                using var webStream = webResponse.GetResponseStream() ?? Stream.Null;
                using var responseReader = new StreamReader(webStream);
                var response = responseReader.ReadToEnd();

                var paymentStatusResponse = JsonConvert.DeserializeObject<PaymentStatusResponse>(response);
                if (paymentStatusResponse == null)
                {
                    await _logger.ErrorAsync("Error getting payment processing. Redirecting to homepage.");
                    return RedirectToRoute("Homepage");
                }

                var paymentStatus = paymentStatusResponse.TransactionStatus;
                var orderId = Convert.ToInt32(paymentStatusResponse.MerchantReference);
                var order = await _orderService.GetOrderByIdAsync(orderId);
                if (order == null)
                {
                    await _logger.ErrorAsync("Error getting order information. Redirecting to homepage.");
                    return RedirectToRoute("Homepage");
                }

                if (paymentStatus.Equals(PoliPayDefaults.COMPLETED))
                {
                    var total = decimal.Zero;
                    try
                    {
                        total = paymentStatusResponse.AmountPaid;
                    }
                    catch (Exception exc)
                    {
                        await _logger.ErrorAsync("Payment Express Process Payment. Error getting AmountSettlement", exc);
                    }

                    var orderTotal = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(order.OrderTotal, await _workContext.GetWorkingCurrencyAsync());
                    //validate order total
                    if (!Math.Round(total, 2).Equals(Math.Round(orderTotal, 2)))
                    {
                        var errorStr = string.Format("Payment Express Process Payment. Returned order total {0} doesn't equal order total {1}",
                                total, order.OrderTotal);
                        await _logger.ErrorAsync(errorStr);
                        return Redirect($"{_webHelper.GetStoreLocation()}orderdetails/{order.Id}");
                    }

                    //order note
                    await _orderService.InsertOrderNoteAsync(new OrderNote
                    {
                        OrderId = order.Id,
                        Note = $"Order payment completed. Token: {token}. Transaction Id: {paymentStatusResponse.TransactionRefNo}",
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow
                    });

                    order.CaptureTransactionId = paymentStatusResponse.TransactionRefNo;
                    await _orderService.UpdateOrderAsync(order);

                    if (order.PaymentStatus != PaymentStatus.Paid)
                    {
                        await _orderProcessingService.MarkOrderAsPaidAsync(order);
                    }
                    return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });
                }
                else
                {
                    return RedirectToRoute("OrderDetails", new { orderId = order.Id });
                }
            }
            catch (Exception e)
            {
                await _logger.InformationAsync(e.Message);
            }
            return RedirectToRoute("Homepage");
        }
    }
}
