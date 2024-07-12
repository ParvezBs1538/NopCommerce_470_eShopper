using System;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Directory;
using NopStation.Plugin.Payments.POLiPay.Models;

namespace NopStation.Plugin.Payments.POLiPay.Factories
{
    public class PolypayModelFactory : IPolypayModelFactory
    {
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly ICurrencyService _currencyService;

        public PolypayModelFactory(IWebHelper webHelper,
            IWorkContext workContext,
            ICurrencyService currencyService)
        {
            _webHelper = webHelper;
            _workContext = workContext;
            _currencyService = currencyService;
        }

        public async Task<PaymentUrlRequest> PreparePaymentUrlRequest(Order order)
        {
            var orderTotal = Math.Round(order.OrderTotal, 2);
            var requestBody = new PaymentUrlRequest
            {
                Amount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(orderTotal, await _workContext.GetWorkingCurrencyAsync()),
                CurrencyCode = (await _workContext.GetWorkingCurrencyAsync()).CurrencyCode,
                NotificationURL = $"{_webHelper.GetStoreLocation()}polipay/postpaymenthandler",
                FailureURL = $"{_webHelper.GetStoreLocation()}polipay/postpaymenthandler",
                SuccessURL = $"{_webHelper.GetStoreLocation()}polipay/postpaymenthandler",
                CancellationURL = $"{_webHelper.GetStoreLocation()}polipay/postpaymenthandler",
                MerchantHomepageURL = $"{_webHelper.GetStoreLocation()}",
                MerchantReference = order.Id.ToString(),
            };
            return requestBody;
        }
    }
}
