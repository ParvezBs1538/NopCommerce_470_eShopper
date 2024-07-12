using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Services.Directory;
using Nop.Services.Payments;
using ILogger = Nop.Services.Logging.ILogger;

namespace NopStation.Plugin.Payments.OABIPayment.Services
{
    public class OABIPaymentService : IOABIPaymentService
    {
        #region Fields

        private readonly OABIPaymentSettings _oabIPaymentSettings;
        private readonly IWorkContext _workContext;
        private readonly ILogger _logger;
        private readonly ICurrencyService _currencyService;
        private readonly IWebHelper _webHelper;

        #endregion

        #region Ctor
        public OABIPaymentService(
            OABIPaymentSettings paykeeperPaymentSettings,
            ILogger logger,
            IWorkContext workContext,
            IWebHelper webHelper)
        {
            _oabIPaymentSettings = paykeeperPaymentSettings;
            _logger = logger;
            _workContext = workContext;
            _webHelper = webHelper;
        }

        #endregion

        #region Methods

        public async Task<string> GetLink(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            try
            {
                var receiptURL = $"{_webHelper.GetStoreLocation()}Plugins/OABIPayment/Success";
                var errorURL = $"{_webHelper.GetStoreLocation()}Plugins/OABIPayment/Fail";
                var currency = await GetCurrencyCode();
                var action = 1; // 1= AuthorizeAndCapture
                var tranrequest = "amt=" + postProcessPaymentRequest.Order.OrderTotal.ToString() +
                                        "&action=" + action +
                                        "&responseURL=" + receiptURL +
                                        "&errorURL=" + errorURL +
                                        "&trackId=" + postProcessPaymentRequest.Order.Id.ToString() +
                                        "&udf1=" + postProcessPaymentRequest.Order.Id.ToString() +
                                        "&udf2=" + postProcessPaymentRequest.Order.CustomerId.ToString() +
                                        "&currencycode=" + currency +
                                        "&langid=EN" +
                                        "&id=" + _oabIPaymentSettings.TranPortalId +
                                        "&password=" + _oabIPaymentSettings.TranPortaPassword +
                                        "&";

                byte[] results;
                StringBuilder sb = null;
                var tripleDESAlgorithm = new TripleDESCryptoServiceProvider() { Mode = CipherMode.ECB, Padding = PaddingMode.Zeros };
                tripleDESAlgorithm.Key = Encoding.ASCII.GetBytes(_oabIPaymentSettings.ResourceKey);
                byte[] dataToEncrypt = Encoding.UTF8.GetBytes(tranrequest);
                try
                {
                    ICryptoTransform encryptor = tripleDESAlgorithm.CreateEncryptor();
                    results = encryptor.TransformFinalBlock(dataToEncrypt, 0, dataToEncrypt.Length);
                    encryptor.Dispose();
                }
                finally
                {
                    tripleDESAlgorithm.Clear();
                }
                sb = new StringBuilder(results.Length * 2);
                foreach (byte b in results)
                {
                    sb.Append(b.ToString("x").PadLeft(2, '0'));
                }

                string encrypttranrequest = "&trandata=" + sb.ToString();
                encrypttranrequest = encrypttranrequest + "&errorURL=" + errorURL;
                encrypttranrequest = encrypttranrequest + "&responseURL=" + receiptURL;
                encrypttranrequest = encrypttranrequest + "&tranportalId=" + _oabIPaymentSettings.TranPortalId;

                return OABIPaymentDefaults.PaymentGatewayEndPoint + encrypttranrequest;
            }
            catch (Exception e)
            {
                await _logger.ErrorAsync(e.Message, e);
                return null;
            }
        }

        //public async Task<string> RefundAsync(string orderCaptureTransactionId, decimal? amount, bool isPartialRefund)
        //{
        //    try
        //    {
        //        dynamic jObject = null;

        //        var req = CreateWebRequest("POST", ""/change/payment/reverse/");
        //        var data = $"&token={await GetToken()}&id={orderCaptureTransactionId}&amount={amount}&partial={isPartialRefund}&refund_cart=[]";
        //        using (var streamWriter = new StreamWriter(await req.GetRequestStreamAsync()))
        //        {
        //            await streamWriter.WriteAsync(data);
        //            await streamWriter.FlushAsync();
        //            streamWriter.Close();

        //            var httpWebResponse = (HttpWebResponse)await req.GetResponseAsync();

        //            using (var streamReader = new StreamReader(httpWebResponse.GetResponseStream()))
        //            {
        //                var response = await streamReader.ReadToEndAsync();
        //                jObject = JObject.Parse(response);
        //            }
        //        }

        //        // retrieving response from dynamic object if any error exist
        //        try
        //        {
        //            var msz = jObject.msg.ToString();
        //            if (!string.IsNullOrWhiteSpace(msz))
        //            {
        //                await _logger.ErrorAsync(msz);
        //                return msz;
        //            }
        //        }
        //        catch (Exception e)
        //        {

        //        }

        //        return null;
        //    }
        //    catch (Exception e)
        //    {
        //        await _logger.ErrorAsync(e.Message, e);
        //        return e.Message;
        //    }

        //}

        #endregion

        #region Utility

        private async Task<string> GetCurrencyCode()
        {
            var currencyCode = (await _workContext.GetWorkingCurrencyAsync()).CurrencyCode.Trim();

            if (OABIPaymentDefaults.CurrencyCodes.ContainsKey(currencyCode))
                return OABIPaymentDefaults.CurrencyCodes.GetValueOrDefault(currencyCode);

            return string.Empty;
        }

        #endregion
    }
}
