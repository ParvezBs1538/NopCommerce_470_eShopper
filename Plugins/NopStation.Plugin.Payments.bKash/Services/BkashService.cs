using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Services.Configuration;
using Nop.Services.Orders;
using NopStation.Plugin.Payments.bKash.Models;

namespace NopStation.Plugin.Payments.bKash.Services
{
    public class BkashService : IBkashService
    {
        #region Fields

        private const string LIVE_URL = "https://checkout.pay.bka.sh/v1.0.0-beta/";
        private const string SANDBOX_URL = "https://checkout.sandbox.bka.sh/v1.2.0-beta/";
        private const string CREATE_PAYMENT_URL = "checkout/payment/create";
        private const string EXECUTE_PAYMENT_URL = "checkout/payment/execute";
        private const string TOKEN_URL = "checkout/token/grant";

        private readonly IOrderService _orderService;
        private readonly BkashPaymentSettings _bkashPaymentSettings;
        private readonly ISettingService _settingService;

        #endregion

        #region Ctor

        public BkashService(ISettingService settingService,
            BkashPaymentSettings bkashAdvancePaymentSettings,
            IOrderService orderService)
        {
            _orderService = orderService;
            _bkashPaymentSettings = bkashAdvancePaymentSettings;
            _settingService = settingService;
        }

        #endregion

        #region Utilities

        protected Uri GetBaseUri()
        {
            return new Uri(_bkashPaymentSettings.UseSandbox ? SANDBOX_URL : LIVE_URL);
        }

        protected virtual async Task<GrantTokenResponse> GetGrantTokenAsync()
        {
            var tokenResponse = PrepareGrantTokenResponse(_bkashPaymentSettings);

            if (IsValidToken(tokenResponse))
                return tokenResponse;

            var postModel = new GrantTokenRequest()
            {
                AppSecret = _bkashPaymentSettings.AppSecret,
                AppKey = _bkashPaymentSettings.AppKey
            };

            var headers = new Dictionary<string, string>();
            headers.Add("Content-Type", "application/json");
            headers.Add("username", _bkashPaymentSettings.Username);
            headers.Add("password", _bkashPaymentSettings.Password);

            var response = GetBaseUri().Concat(TOKEN_URL).Post<GrantTokenResponse>(headers, model: postModel);

            _bkashPaymentSettings.RefreshToken = response.Model.RefreshToken;
            _bkashPaymentSettings.TokenCreateTime = DateTime.UtcNow;
            _bkashPaymentSettings.TokenType = response.Model.TokenType;
            _bkashPaymentSettings.IdToken = response.Model.IdToken;
            _bkashPaymentSettings.ExpiresInSec = response.Model.ExpiresIn;

            await _settingService.SaveSettingAsync(_bkashPaymentSettings);

            return PrepareGrantTokenResponse(_bkashPaymentSettings);
        }

        private GrantTokenResponse PrepareGrantTokenResponse(BkashPaymentSettings bkashPaymentSettings)
        {
            return new GrantTokenResponse()
            {
                ExpiresIn = bkashPaymentSettings.ExpiresInSec,
                IdToken = bkashPaymentSettings.IdToken,
                RefreshToken = bkashPaymentSettings.RefreshToken,
                TokenType = bkashPaymentSettings.TokenType,
                TokenCreateTime = bkashPaymentSettings.TokenCreateTime
            };
        }

        protected virtual bool IsValidToken(GrantTokenResponse model)
        {
            if (!string.IsNullOrWhiteSpace(model.TokenType) && model.TokenCreateTime.AddSeconds(model.ExpiresIn).AddMinutes(-10) >= DateTime.UtcNow)
                return true;
            return false;
        }

        protected async Task<Dictionary<string, string>> PrepareHeadersAsync()
        {
            var tokenResponse = await GetGrantTokenAsync();

            var headers = new Dictionary<string, string>();
            headers.Add("Content-Type", "application/json");
            headers.Add("Authorization", tokenResponse.IdToken);
            headers.Add("X-APP-Key", _bkashPaymentSettings.AppKey);
            return headers;
        }

        #endregion

        #region Methods

        public virtual async Task<PaymentResponse> CreatePaymentAsync(Order order)
        {
            var postModel = new PaymentRequest()
            {
                Amount = decimal.Round(order.OrderTotal, 2, MidpointRounding.AwayFromZero).ToString(),
                Currency = "BDT",
                Intent = "sale",
                MerchantInvoiceNumber = order.Id.ToString()
            };

            var model = GetBaseUri().Concat(CREATE_PAYMENT_URL).Post<PaymentResponse>(await PrepareHeadersAsync(), model: postModel);
            order.AuthorizationTransactionCode = model.Model.PaymentID;
            order.AuthorizationTransactionResult = model.Model.TransactionStatus;
            await _orderService.UpdateOrderAsync(order);

            return model.Model;
        }

        public virtual async Task<PaymentResponse> ExecutePaymentAsync(Order order)
        {
            var uri = GetBaseUri().Concat(EXECUTE_PAYMENT_URL).Concat(order.AuthorizationTransactionCode);
            var model = uri.Post<PaymentResponse>(await PrepareHeadersAsync());
            order.AuthorizationTransactionId = model.Model.TransactionId;
            order.AuthorizationTransactionResult = model.Model.TransactionStatus;

            if (model.Model.TransactionStatus == "Completed" || model.Model.TransactionStatus == "Authorized")
            {
                if (model.Model.TransactionStatus == "Completed")
                    order.PaymentStatus = PaymentStatus.Paid;
                else
                    order.PaymentStatus = PaymentStatus.Authorized;
                order.OrderStatus = OrderStatus.Processing;
            }

            await _orderService.UpdateOrderAsync(order);

            return model.Model;
        }

        #endregion
    }
}
