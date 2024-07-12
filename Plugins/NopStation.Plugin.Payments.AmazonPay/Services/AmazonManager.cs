using System.Threading.Tasks;
using Amazon.Pay.API;
using Amazon.Pay.API.Types;
using Amazon.Pay.API.WebStore;
using Amazon.Pay.API.WebStore.CheckoutSession;
using Amazon.Pay.API.WebStore.Refund;
using Amazon.Pay.API.WebStore.Types;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Orders;
using Nop.Services.Payments;

namespace NopStation.Plugin.Payments.AmazonPay.Services
{
    public class AmazonManager
    {
        private readonly IOrderService _orderService;
        private readonly ICurrencyService _currencyService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IAddressService _addressService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ICountryService _countryService;
        private readonly IWebHelper _webHelper;
        private readonly ISettingService _settingService;
        private readonly IPriceCalculationService _priceCalculationService;

        public AmazonManager(IOrderService orderService,
            ICurrencyService currencyService,
            IOrderProcessingService orderProcessingService,
            IWorkContext workContext,
            IStoreContext storeContext,
            IAddressService addressService,
            IStateProvinceService stateProvinceService,
            ICountryService countryService,
            IWebHelper webHelper,
            ISettingService settingService,
            IPriceCalculationService priceCalculationService)
        {
            _orderService = orderService;
            _currencyService = currencyService;
            _orderProcessingService = orderProcessingService;
            _workContext = workContext;
            _storeContext = storeContext;
            _addressService = addressService;
            _stateProvinceService = stateProvinceService;
            _countryService = countryService;
            _webHelper = webHelper;
            _settingService = settingService;
            _priceCalculationService = priceCalculationService;
        }

        protected WebStoreClient GetWebStoreClient(AmazonPaySettings amazonPaySettings)
        {
            // set up config
            var payConfiguration = new ApiConfiguration
            (
                region: (Region)amazonPaySettings.RegionId,
                environment: amazonPaySettings.UseSandbox ? Environment.Sandbox : Environment.Live,
                publicKeyId: amazonPaySettings.PublicKeyId,
                privateKey: amazonPaySettings.PrivateKey
            );

            // init API client
            return new WebStoreClient(payConfiguration);
        }

        public async Task<(string Payload, string Signature, AmazonPaySettings AmazonPaySettings)> InitializeOrderAsync(Order order)
        {
            var amazonPaySettings = await _settingService.LoadSettingAsync<AmazonPaySettings>(order.StoreId);

            var request = new CreateCheckoutSessionRequest();
            request.StoreId = amazonPaySettings.StoreId;
            request.WebCheckoutDetails.CheckoutResultReturnUrl = $"{_webHelper.GetStoreLocation()}amazonpay/callback/{order.OrderGuid}";
            request.WebCheckoutDetails.CheckoutMode = CheckoutMode.ProcessOrder;

            var currency = AmazonPayHelper.GetCurrencyEnum(order.CustomerCurrencyCode);
            var orderTotal = _currencyService.ConvertCurrency(order.OrderTotal, order.CurrencyRate);

            // set payment details
            request.PaymentDetails.PaymentIntent = PaymentIntent.AuthorizeWithCapture;
            request.PaymentDetails.ChargeAmount.Amount = orderTotal;
            request.PaymentDetails.ChargeAmount.CurrencyCode = currency;
            request.PaymentDetails.PresentmentCurrency = currency;

            // set meta data
            request.MerchantMetadata.MerchantReferenceId = order.OrderGuid.ToString();
            request.MerchantMetadata.MerchantStoreName = _storeContext.GetCurrentStore().Name;
            request.MerchantMetadata.NoteToBuyer = amazonPaySettings.NoteToBuyer;

            var address = await _addressService.GetAddressByIdAsync(order.BillingAddressId);
            var state = await _stateProvinceService.GetStateProvinceByAddressAsync(address);
            var country = await _countryService.GetCountryByAddressAsync(address);

            // submit shipping address entered by buyer on merchant website
            request.AddressDetails.Name = $"{address.FirstName} {address.LastName}";
            request.AddressDetails.AddressLine1 = address.Address1;
            request.AddressDetails.AddressLine2 = address.Address2;
            request.AddressDetails.City = address.City;
            request.AddressDetails.StateOrRegion = state?.Abbreviation;
            request.AddressDetails.PostalCode = address.ZipPostalCode;
            request.AddressDetails.CountryCode = country?.TwoLetterIsoCode;
            request.AddressDetails.PhoneNumber = address.PhoneNumber;

            // set up config
            var client = GetWebStoreClient(amazonPaySettings);

            return (request.ToJson(), client.GenerateButtonSignature(request), amazonPaySettings);
        }

        public async Task CompleteCheckoutSessionAsync(Order order, string amazonCheckoutSessionId)
        {
            var amazonPaySettings = await _settingService.LoadSettingAsync<AmazonPaySettings>(order.StoreId);

            var currency = AmazonPayHelper.GetCurrencyEnum(order.CustomerCurrencyCode);
            var orderTotal = await _priceCalculationService.RoundPriceAsync(_currencyService.ConvertCurrency(order.OrderTotal, order.CurrencyRate),
                await _currencyService.GetCurrencyByCodeAsync(order.CustomerCurrencyCode));

            var request = new CompleteCheckoutSessionRequest(orderTotal, currency);

            // set up config
            var client = GetWebStoreClient(amazonPaySettings);

            // send the request
            var result = client.CompleteCheckoutSession(amazonCheckoutSessionId, request);

            // check if API call was successful
            if (result.Success && _orderProcessingService.CanMarkOrderAsPaid(order))
            {
                await _orderProcessingService.MarkOrderAsPaidAsync(order);

                order.CaptureTransactionId = result.ChargeId;
                await _orderService.UpdateOrderAsync(order);
            }
        }

        public async Task<RefundResponse> RefundAsync(RefundPaymentRequest refundPaymentRequest)
        {
            var order = refundPaymentRequest.Order;
            var amazonPaySettings = await _settingService.LoadSettingAsync<AmazonPaySettings>(order.StoreId);

            var currency = AmazonPayHelper.GetCurrencyEnum(order.CustomerCurrencyCode);
            var amountToRefund = _currencyService.ConvertCurrency(refundPaymentRequest.AmountToRefund, order.CurrencyRate);
            var request = new CreateRefundRequest(order.CaptureTransactionId, amountToRefund, currency);

            // set up config
            var client = GetWebStoreClient(amazonPaySettings);

            // send the request
            return client.CreateRefund(request);
        }
    }
}
