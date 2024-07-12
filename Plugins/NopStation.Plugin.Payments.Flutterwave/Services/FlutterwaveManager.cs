using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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
using Nop.Web.Framework;
using RestSharp;

namespace NopStation.Plugin.Payments.Flutterwave.Services
{
    public class FlutterwaveManager
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
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICountryService _countryService;

        #endregion

        #region Ctor

        public FlutterwaveManager(IOrderService orderService,
            ICurrencyService currencyService,
            IOrderProcessingService orderProcessingService,
            IWorkContext workContext,
            IAddressService addressService,
            IWebHelper webHelper,
            ISettingService settingService,
            ILogger logger,
            IHttpContextAccessor httpContextAccessor,
            ICountryService countryService)
        {
            _orderService = orderService;
            _currencyService = currencyService;
            _orderProcessingService = orderProcessingService;
            _workContext = workContext;
            _addressService = addressService;
            _webHelper = webHelper;
            _settingService = settingService;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _countryService = countryService;
        }

        #endregion

        #region Utilities

        protected Uri GetbaseUrl(FlutterwavePaymentSettings flutterwavePaymentSettings)
        {
            if (flutterwavePaymentSettings.UseSandbox)
                return new Uri("https://checkout.flutterwave.com/");

            return new Uri("https://checkout.flutterwave.com/");
        }

        #endregion

        #region Methods

        public async Task RemotePost(Order order)
        {
            try
            {
                var flutterwavePaymentSettings = await _settingService.LoadSettingAsync<FlutterwavePaymentSettings>(order.StoreId);

                var amount = _currencyService.ConvertCurrency(order.OrderTotal, order.CurrencyRate);
                var address = await _addressService.GetAddressByIdAsync(order.BillingAddressId);

                var url = GetbaseUrl(flutterwavePaymentSettings).Concat("/v3/hosted/pay");
                var returnUrl = $"{_webHelper.GetStoreLocation()}flutterwave/return/{order.Id}";
                ;

                var remotePost = new RemotePost(_httpContextAccessor, _webHelper);
                remotePost.Url = url.AbsoluteUri;
                remotePost.Method = "POST";
                remotePost.FormName = "FlutterwaveForm";

                remotePost.Params.Add("public_key", flutterwavePaymentSettings.PublicKey);
                remotePost.Params.Add("customer[email]", address.Email);
                remotePost.Params.Add("customer[name]", $"{address.FirstName} {address.LastName}");
                remotePost.Params.Add("redirect_url", returnUrl);
                remotePost.Params.Add("tx_ref", order.OrderGuid.ToString());
                remotePost.Params.Add("currency", order.CustomerCurrencyCode);
                remotePost.Params.Add("amount", amount.ToString("#.##"));

                remotePost.Post();
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync(ex.Message, ex);
            }
        }

        public async Task VerifyPaymentAsync(Order order, string transactionId)
        {
            try
            {
                var flutterwavePaymentSettings = await _settingService.LoadSettingAsync<FlutterwavePaymentSettings>(order.StoreId);

                var request = new RestRequest(Method.GET)
                {
                    RequestFormat = DataFormat.Json
                };

                request.AddHeader("Authorization", $"Bearer {flutterwavePaymentSettings.SecretKey}");

                var client = new RestClient($"https://api.flutterwave.com/v3/transactions/{transactionId}/verify");
                var response = client.Execute(request);

                var verificationResponse = JsonConvert.DeserializeObject<PaymentVerificationResponse>(response.Content);

                if (verificationResponse.Status.Equals("success", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (order.OrderGuid == Guid.Parse(verificationResponse.Data.OrderGuid))
                    {
                        var amount = _currencyService.ConvertCurrency(order.OrderTotal, order.CurrencyRate);
                        if (verificationResponse.Data.Amount >= amount)
                        {
                            await _orderProcessingService.MarkOrderAsPaidAsync(order);

                            order.AuthorizationTransactionId = transactionId;
                            order.AuthorizationTransactionResult = verificationResponse.Status;
                            order.AuthorizationTransactionCode = verificationResponse.Data.FlwRef;

                            order.CardName = verificationResponse.Data.Card.Type;
                            order.CardNumber = $"{verificationResponse.Data.Card.First6digits}******{verificationResponse.Data.Card.Last4digits}";

                            await _orderService.UpdateOrderAsync(order);

                            await _orderService.InsertOrderNoteAsync(new OrderNote()
                            {
                                OrderId = order.Id,
                                CreatedOnUtc = DateTime.UtcNow,
                                Note = verificationResponse.Message
                            });
                        }
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
                var flutterwavePaymentSettings = await _settingService.LoadSettingAsync<FlutterwavePaymentSettings>(order.StoreId);

                var request = new RestRequest(Method.POST)
                {
                    RequestFormat = DataFormat.Json
                };

                request.AddHeader("Authorization", $"Bearer {flutterwavePaymentSettings.SecretKey}");

                var amount = _currencyService.ConvertCurrency(refundPaymentRequest.AmountToRefund, order.CurrencyRate);

                request.AddJsonBody(new { amount = amount, comments = "Refunded by admin." });

                var client = new RestClient($"https://api.flutterwave.com/v3/transactions/{order.AuthorizationTransactionId}/refund");
                var response = client.Execute(request);

                var refundResponse = JsonConvert.DeserializeObject<RefundResponse>(response.Content);

                if (refundResponse.Status.Equals("success", StringComparison.InvariantCultureIgnoreCase))
                {
                    result.NewPaymentStatus = refundPaymentRequest.IsPartialRefund ?
                        PaymentStatus.PartiallyRefunded : PaymentStatus.Refunded;

                    await _orderService.InsertOrderNoteAsync(new OrderNote()
                    {
                        OrderId = order.Id,
                        CreatedOnUtc = DateTime.UtcNow,
                        Note = refundResponse.Message
                    });
                }
                else
                {
                    result.AddError(refundResponse.Message);
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
