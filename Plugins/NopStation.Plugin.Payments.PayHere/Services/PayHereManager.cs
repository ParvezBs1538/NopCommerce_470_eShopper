using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Web.Framework;

namespace NopStation.Plugin.Payments.PayHere.Services
{
    public class PayHereManager
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

        public PayHereManager(IOrderService orderService,
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

        protected string GetMD5(string str)
        {
            using (var md5 = MD5.Create())
            {
                var inputBytes = Encoding.ASCII.GetBytes(str);
                var hashBytes = md5.ComputeHash(inputBytes);

                var sb = new StringBuilder();
                for (var i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }

        protected Uri GetbaseUrl(PayHerePaymentSettings payHerePaymentSettings)
        {
            if (payHerePaymentSettings.UseSandbox)
                return new Uri("https://sandbox.payhere.lk");

            return new Uri("https://www.payhere.lk");
        }

        #endregion

        #region Methods

        public async Task RemotePost(Order order)
        {
            try
            {
                var payHerePaymentSettings = await _settingService.LoadSettingAsync<PayHerePaymentSettings>(order.StoreId);

                var amount = _currencyService.ConvertCurrency(order.OrderTotal, order.CurrencyRate);
                var address = await _addressService.GetAddressByIdAsync(order.BillingAddressId);

                var url = GetbaseUrl(payHerePaymentSettings).Concat("/pay/checkout");
                var returnUrl = $"{_webHelper.GetStoreLocation()}payhere/return/{order.Id}/return";
                var cancelUrl = $"{_webHelper.GetStoreLocation()}payhere/return/{order.Id}/cancel";
                var notifyUrl = $"{_webHelper.GetStoreLocation()}payhere/notify/{order.Id}";

                var remotePost = new RemotePost(_httpContextAccessor, _webHelper);
                remotePost.Url = url.AbsoluteUri;
                remotePost.Method = "POST";
                remotePost.FormName = "PayHereForm";

                remotePost.Params.Add("merchant_id", payHerePaymentSettings.MerchantId);
                remotePost.Params.Add("return_url", returnUrl);
                remotePost.Params.Add("cancel_url", cancelUrl);
                remotePost.Params.Add("notify_url", notifyUrl);
                remotePost.Params.Add("order_id", order.Id.ToString());
                remotePost.Params.Add("currency", order.CustomerCurrencyCode);
                remotePost.Params.Add("amount", amount.ToString("#.##"));
                remotePost.Params.Add("first_name", address.FirstName);
                remotePost.Params.Add("last_name", address.LastName);
                remotePost.Params.Add("items", $"Online purchase, Order #{order.Id}");
                remotePost.Params.Add("email", address.Email);
                remotePost.Params.Add("phone", address.PhoneNumber);
                remotePost.Params.Add("address", address.Address1);
                remotePost.Params.Add("city", address.City);
                remotePost.Params.Add("country", (await _countryService.GetCountryByAddressAsync(address))?.Name);

                remotePost.Post();
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync(ex.Message, ex);
            }
        }

        public async Task VerifyPaymentAsync(Order order, IQueryCollection query)
        {
            try
            {
                var payHerePaymentSettings = await _settingService.LoadSettingAsync<PayHerePaymentSettings>(order.StoreId);

                var merchant_id = query["merchant_id"].ToString();
                var order_id = query["order_id"].ToString();
                var payhere_amount = query["payhere_amount"].ToString();
                var payhere_currency = query["payhere_currency"].ToString();
                var status_code = query["status_code"].ToString();
                var md5sig = query["md5sig"].ToString();

                var str = merchant_id + order_id + payhere_amount + payhere_currency + status_code + GetMD5(payHerePaymentSettings.MerchantSecret).ToUpper();
                var signature = GetMD5(str).ToUpper();

                if (signature == md5sig && _orderProcessingService.CanMarkOrderAsPaid(order))
                {
                    await _orderProcessingService.MarkOrderAsPaidAsync(order);

                    order.AuthorizationTransactionId = query["payment_id"].ToString();
                    await _orderService.UpdateOrderAsync(order);
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

                var payHerePaymentSettings = await _settingService.LoadSettingAsync<PayHerePaymentSettings>(order.StoreId);

                var url = GetbaseUrl(payHerePaymentSettings).Concat("/api/1.1/refunds/");

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
