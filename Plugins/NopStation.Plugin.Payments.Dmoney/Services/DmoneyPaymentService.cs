using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Services.Localization;
using Nop.Services.Orders;
using NopStation.Plugin.Payments.Dmoney.Domains;
using NopStation.Plugin.Payments.Dmoney.Models;

namespace NopStation.Plugin.Payments.Dmoney.Services
{
    public class DmoneyPaymentService : IDmoneyPaymentService
    {
        private readonly IOrderService _orderService;
        private readonly IDmoneyTransactionService _dmoneyTransactionService;
        private readonly DmoneyPaymentSettings _dmoneyPaymentSettings;
        private readonly ILocalizationService _localizationService;

        public DmoneyPaymentService(IOrderService orderService,
            IDmoneyTransactionService dmoneyTransactionService,
            DmoneyPaymentSettings dmoneyPaymentSettings,
            ILocalizationService localizationService)
        {
            _orderService = orderService;
            _dmoneyTransactionService = dmoneyTransactionService;
            _dmoneyPaymentSettings = dmoneyPaymentSettings;
            _localizationService = localizationService;
        }

        public virtual async Task<VerifyTransactionResult> VerifyTransactionAsync(string transactionTrackingNo)
        {
            var result = new VerifyTransactionResult();
            var transaction = await _dmoneyTransactionService.GetTransactionByTrackingNumberAsync(transactionTrackingNo);
            if (transaction == null)
            {
                result.AddError(await _localizationService.GetResourceAsync("NopStation.DmoneyPayment.Transaction.TransactionNotFound"));
                return result;
            }

            var order = await _orderService.GetOrderByIdAsync(transaction.OrderId);
            if (order == null || order.Deleted)
            {
                result.AddError(await _localizationService.GetResourceAsync("NopStation.DmoneyPayment.Transaction.OrderNotFound"));
                return result;
            }

            var postModel = new { transactionTrackingNo };

            var jsonModel = JsonConvert.SerializeObject(postModel);
            var data = Encoding.ASCII.GetBytes(jsonModel);

            var request = (HttpWebRequest)WebRequest.Create(_dmoneyPaymentSettings.TransactionVerificationUrl);

            request.Method = "POST";
            request.ContentType = "application/json";
            request.Accept = "application/json";
            request.ContentLength = data.Length;

            request.Headers.Add("orgCode", _dmoneyPaymentSettings.OrganizationCode);
            request.Headers.Add("password", _dmoneyPaymentSettings.Password);
            request.Headers.Add("secretKey", _dmoneyPaymentSettings.SecretKey);

            using (Stream stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse)request.GetResponse();
            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

            var responseModel = JsonConvert.DeserializeObject<TransactionStatusResponse>(responseString);

            transaction.LastUpdatedOnUtc = DateTime.UtcNow;
            transaction.MerchantWalletNumber = responseModel.Data.MerchantWalletNumber;
            transaction.PaymentStatus = responseModel.Data.PaymentStatus;
            transaction.StatusCode = responseModel.StatusCode;
            transaction.StatusMessage = responseModel.Data.StatusMessage;
            transaction.TransactionReferenceId = responseModel.Data.TransactionReferenceId;
            transaction.TransactionTime = responseModel.Data.TransactionTime;
            transaction.TransactionType = responseModel.Data.TransactionType;
            transaction.Amount = responseModel.Data.Amount;
            transaction.CustomerWalletNumber = responseModel.Data.CustomerWalletNumber;
            transaction.ErrorMessage = responseModel.Error.Message;
            transaction.ErrorCode = responseModel.Error.ErrorCode;

            await _dmoneyTransactionService.UpdateTransactionAsync(transaction);

            if (responseModel.StatusCode == 200 && responseModel.Data.StatusCode == "SUCCESS" &&
                responseModel.Data.PaymentStatus == "COMPLETED" && responseModel.Data.Amount >= order.OrderTotal)
            {
                order.PaymentStatus = PaymentStatus.Paid;
                order.OrderStatus = OrderStatus.Processing;
                await _orderService.UpdateOrderAsync(order);

                result.Status = true;
                result.OrderId = order.Id;
                return result;
            }

            result.AddError(await _localizationService.GetResourceAsync("NopStation.DmoneyPayment.Transaction.Invalid"));
            return result;
        }
    }
}
