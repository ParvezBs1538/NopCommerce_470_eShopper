using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Common;
using Nop.Services.Directory;
using Nop.Services.Logging;
using Nop.Services.Stores;
using NopStation.Plugin.Payments.AamarPay.Models;

namespace NopStation.Plugin.Payments.AamarPay.Services;

public class AamarPayPaymentService : IAamarPayPaymentService
{
    #region Fields

    private readonly AamarPayPaymentSettings _aamarPayPaymentSettings;
    private readonly ILogger _logger;
    private readonly IWebHelper _webHelper;
    private readonly IWorkContext _workContext;
    private readonly IStoreService _storeService;
    private readonly ICurrencyService _currencyService;
    private readonly IAddressService _addressService;
    private readonly IStateProvinceService _stateProvinceService;

    #endregion

    #region Ctor

    public AamarPayPaymentService(AamarPayPaymentSettings aamarPayPaymentSettings,
        ILogger logger,
        IWebHelper webHelper,
        IWorkContext workContext,
        IStoreService storeService,
        ICurrencyService currencyService,
        IAddressService addressService,
        IStateProvinceService stateProvinceService)
    {
        _aamarPayPaymentSettings = aamarPayPaymentSettings;
        _logger = logger;
        _webHelper = webHelper;
        _workContext = workContext;
        _storeService = storeService;
        _currencyService = currencyService;
        _addressService = addressService;
        _stateProvinceService = stateProvinceService;
    }

    #endregion

    #region Utilities

    private async Task<T> SendApiRequestAsync<T>(string path, HttpMethod httpMethod, string errorMessage, string serializedContent = null)
    {
        var baseUrl = _aamarPayPaymentSettings.UseSandbox ? AamarPayPaymentDefaults.SANDBOX_BASE_URL : AamarPayPaymentDefaults.LIVE_BASE_URL;
        var requestPath = baseUrl + path;

        var client = new HttpClient();
        var request = new HttpRequestMessage(httpMethod, requestPath);

        if (serializedContent != null)
        {
            request.Content = new StringContent(serializedContent);
        }

        try
        {
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var stringResponse = await response.Content.ReadAsStringAsync();
            try
            {
                var desirializeResponse = JsonConvert.DeserializeObject<T>(stringResponse);
                return desirializeResponse;
            }
            catch (Exception ex)
            {
                _logger.Error(errorMessage + stringResponse, ex);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(errorMessage, ex);
        }
        return default;
    }

    #endregion

    #region Methods

    public async Task<PaymentInitResponseModel> AamarPayPaymentInitAsync(Order order)
    {
        var signatureKey = _aamarPayPaymentSettings.SignatureKey;
        var paymentInitPath = AamarPayPaymentDefaults.PAYMENT_INIT_PATH;
        var paymentInitErrorMessage = "AamarPay payment init API reques failed";

        var storeLocation = _webHelper.GetStoreLocation();
        var store = await _storeService.GetStoreByIdAsync(order.StoreId);
        var storeId = _aamarPayPaymentSettings.MerchantId;
        var billingAddress = await _addressService.GetAddressByIdAsync(order.BillingAddressId);
        var state = await _stateProvinceService.GetStateProvinceByAddressAsync(billingAddress);
        var orderTotal = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(order.OrderTotal, await _workContext.GetWorkingCurrencyAsync());

        var currentCustomerCurrencyId = (await _workContext.GetCurrentCustomerAsync()).CurrencyId;
        var currencyTmp = await _currencyService.GetCurrencyByIdAsync(currentCustomerCurrencyId ?? 0);
        var customerCurrency = currencyTmp != null && currencyTmp.Published ? currencyTmp : await _workContext.GetWorkingCurrencyAsync();

        var paymentInitRequestModel = new PaymentInitRequestModel
        {
            StoreId = storeId,
            TransactionId = order.OrderGuid.ToString(),
            SuccessUrl = $"{storeLocation}AamarPayPayment/Success",
            FailUrl = $"{storeLocation}AamarPayPayment/Failure",
            CancelUrl = $"{storeLocation}AamarPayPayment/Cancelled",
            Amount = orderTotal.ToString(),
            Currency = customerCurrency.CurrencyCode,
            SignatureKey = signatureKey,
            Description = $"Payment from {store.Name}",
            CustomerName = billingAddress.FirstName + " " + billingAddress.LastName,
            CustomerEmail = billingAddress.Email,
            CustomerAddress1 = billingAddress.Address1,
            CustomerAddress2 = billingAddress.Address2,
            CustomerCity = billingAddress.City,
            CustomerState = state?.Name,
            CustomerPostcode = billingAddress.ZipPostalCode,
            CustomerCountry = billingAddress.County,
            CustomerPhone = billingAddress.PhoneNumber,
            Type = "json"
        };

        var serializedPaymentInitRequest = JsonConvert.SerializeObject(paymentInitRequestModel);

        var paymentInitResponse = await SendApiRequestAsync<PaymentInitResponseModel>(paymentInitPath, HttpMethod.Post, paymentInitErrorMessage, serializedPaymentInitRequest);

        return paymentInitResponse;
    }

    #endregion
}
