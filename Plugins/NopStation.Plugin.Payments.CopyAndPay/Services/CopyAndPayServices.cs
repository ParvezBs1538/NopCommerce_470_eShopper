using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Services.Directory;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using NopStation.Plugin.Payments.CopyAndPay.Models;

namespace NopStation.Plugin.Payments.CopyAndPay.Services
{
    public class CopyAndPayServices : ICopyAndPayServices
    {
        #region Fields

        private readonly ILogger _logger;
        private readonly CopyAndPayPaymentSettings _cOPYandPayPaymentSettings;
        private readonly IWorkContext _workContext;
        private readonly ICurrencyService _currencyService;
        private readonly IOrderService _orderService;

        #endregion

        #region Ctor

        public CopyAndPayServices(ILogger logger,
             CopyAndPayPaymentSettings cOPYandPayPaymentSettings,
             IWorkContext workContext,
             ICurrencyService currencyService,
             IOrderService orderService)
        {
            _logger = logger;
            _cOPYandPayPaymentSettings = cOPYandPayPaymentSettings;
            _workContext = workContext;
            _currencyService = currencyService;
            _orderService = orderService;
        }

        #endregion

        #region Methods

        public async Task<Dictionary<string, dynamic>> RequestFormAsync(int orderId, string selectedBrand)
        {
            Dictionary<string, dynamic> responseData = null;
            var workingCurrency = await _workContext.GetWorkingCurrencyAsync();
            var currency = workingCurrency.CurrencyCode;

            var order = await _orderService.GetOrderByIdAsync(orderId);
            var priceValue = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(order.OrderTotal, await _workContext.GetWorkingCurrencyAsync());
            priceValue = Math.Round(priceValue, 2);
            var entityId = selectedBrand == "MADA" ? _cOPYandPayPaymentSettings.MadaEntityId : _cOPYandPayPaymentSettings.EntityId;

            try
            {
                var data = $"entityId={entityId}" +
                              $"&amount={priceValue}" +
                              $"&currency={currency}" +
                              "&paymentType=DB" +
                              $"&merchantTransactionId={orderId}" +
                              $"&testMode={_cOPYandPayPaymentSettings.TestMode}";

                var urlIsValid = _cOPYandPayPaymentSettings.APIUrl.Substring(_cOPYandPayPaymentSettings.APIUrl.Length - 1) == "/";
                var url = urlIsValid ? _cOPYandPayPaymentSettings.APIUrl + "v1/checkouts" : _cOPYandPayPaymentSettings.APIUrl + "/v1/checkouts";

                var buffer = Encoding.ASCII.GetBytes(data);
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.Headers["Authorization"] = _cOPYandPayPaymentSettings.AuthorizationKey;
                request.ContentType = "application/x-www-form-urlencoded";
                var postData = request.GetRequestStream();
                postData.Write(buffer, 0, buffer.Length);
                postData.Close();
                using var response = (HttpWebResponse)request.GetResponse();
                var dataStream = response.GetResponseStream();
                var reader = new StreamReader(dataStream);
                responseData = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(reader.ReadToEnd());
                reader.Close();
                dataStream.Close();

                order.CardType = selectedBrand;
                await _orderService.UpdateOrderAsync(order);
            }
            catch (Exception e)
            {
                await _logger.ErrorAsync(e.Message);
            }

            return responseData;
        }

        public bool RequestPaymentStatus(int orderId, string id, out ValidatePayment responseData)
        {
            responseData = null;
            try
            {
                var entityId = _cOPYandPayPaymentSettings.MadaEntityId;
                var order = _orderService.GetOrderByIdAsync(orderId).Result;

                if (order != null && !string.IsNullOrEmpty(order.CardType) && order.CardType != "MADA")
                    entityId = _cOPYandPayPaymentSettings.EntityId;

                var data = $"entityId={entityId}";

                var urlIsValid = _cOPYandPayPaymentSettings.APIUrl.Substring(_cOPYandPayPaymentSettings.APIUrl.Length - 1) == "/";
                var url = urlIsValid
                    ? _cOPYandPayPaymentSettings.APIUrl + $"v1/checkouts/{id}/payment?" + data
                    : _cOPYandPayPaymentSettings.APIUrl + $"/v1/checkouts/{id}/payment?" + data;

                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                request.Headers["Authorization"] = _cOPYandPayPaymentSettings.AuthorizationKey;
                using var response = (HttpWebResponse)request.GetResponse();
                var dataStream = response.GetResponseStream();
                var reader = new StreamReader(dataStream);
                var respJson = reader.ReadToEnd();
                reader.Close();
                dataStream.Close();
                responseData = JsonConvert.DeserializeObject<ValidatePayment>(respJson);
                return true;
            }
            catch (Exception e)
            {
                _logger.ErrorAsync(e.Message);
                return false;
            }
        }

        public bool RefundPayment(RefundPaymentRequest refundPaymentRequest, out RefundPayment responseData)
        {
            responseData = null;
            try
            {
                var entityId = _cOPYandPayPaymentSettings.MadaEntityId;
                if (refundPaymentRequest.Order != null && !string.IsNullOrEmpty(refundPaymentRequest.Order.CardType) && refundPaymentRequest.Order.CardType != "MADA")
                    entityId = _cOPYandPayPaymentSettings.EntityId;

                var data = $"entityId={entityId}" +
                             $"&amount={Math.Round(refundPaymentRequest.AmountToRefund, 2)}" +
                             $"&currency={refundPaymentRequest.Order.CustomerCurrencyCode}" +
                             "&paymentType=RF";

                var urlIsValid = _cOPYandPayPaymentSettings.APIUrl.Substring(_cOPYandPayPaymentSettings.APIUrl.Length - 1) == "/";
                var url = urlIsValid ? _cOPYandPayPaymentSettings.APIUrl + "v1/payments/" : _cOPYandPayPaymentSettings.APIUrl + "/v1/payments/";
                url += refundPaymentRequest.Order.AuthorizationTransactionId;

                var buffer = Encoding.ASCII.GetBytes(data);
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.Headers["Authorization"] = _cOPYandPayPaymentSettings.AuthorizationKey;
                request.ContentType = "application/x-www-form-urlencoded";
                var postData = request.GetRequestStream();
                postData.Write(buffer, 0, buffer.Length);
                postData.Close();

                using var response = (HttpWebResponse)request.GetResponse();
                var dataStream = response.GetResponseStream();
                var reader = new StreamReader(dataStream);
                var respJson = reader.ReadToEnd();
                reader.Close();
                dataStream.Close();
                responseData = JsonConvert.DeserializeObject<RefundPayment>(respJson);
                return true;
            }
            catch (Exception e)
            {
                _logger.ErrorAsync(e.Message);
                return false;
            }
        }

        #endregion
    }
}
