using System;
using System.Threading.Tasks;
using Heidelpay.Payment;
using Heidelpay.Payment.Extensions;
using Heidelpay.Payment.Interfaces;
using Heidelpay.Payment.PaymentTypes;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core;

namespace NopStation.Plugin.Payments.Unzer.Services
{
    public class UnzerHelperService : IUnzerHelperService
    {
        #region Fields

        private readonly IWebHelper _webHelper;
        private readonly UnzerPaymentSettings _unzerPaymentSettings;
        private IHeidelpay _unzer;

        #endregion

        #region Ctor

        public UnzerHelperService(IWebHelper webHelper,
            UnzerPaymentSettings unzerPaymentSettings)
        {
            _webHelper = webHelper;
            _unzerPaymentSettings = unzerPaymentSettings;
        }

        #endregion

        #region Common

        protected IHeidelpay GetHeidelpay()
        {
            if (_unzer != null)
                return _unzer;

            var services = new ServiceCollection();

            services.AddHeidelpay(opt =>
            {
                opt.ApiEndpoint = new Uri(_unzerPaymentSettings.ApiEndpoint);
                opt.ApiVersion = _unzerPaymentSettings.ApiVersion;
                opt.ApiKey = _unzerPaymentSettings.ApiPrivateKey;
            });

            var serviceProvider = services.BuildServiceProvider();
            _unzer = serviceProvider.GetService<IHeidelpay>();

            return _unzer ?? throw new NopException("Can not resolve unzer client instance");
        }

        public async Task<Cancel> CancelAuthorizationByPaymentIdAsync(string paymentId)
        {
            var unzer = GetHeidelpay();
            if (unzer == null)
                throw new NopException("Can not resolve unzer client instance");

            return await unzer.CancelAuthorizationAsync(paymentId);
        }

        public async Task<Payment> FetchPaymentByPaymentIdAsync(string paymentId)
        {
            return await GetHeidelpay().FetchPaymentAsync(paymentId);
        }

        public async Task<Charge> CreateChargeByAuthorizationAsync(Authorization authorization)
        {
            return await authorization.ChargeAsync();
        }

        public async Task<Charge> FetchChargeByPaymentIdAndChargeIdAsync(string paymentId, string chargeId)
        {
            return await GetHeidelpay().FetchChargeAsync(paymentId, chargeId);
        }

        public async Task<Cancel> CancelChargeByPaymentIdAndChargeIdAsync(string paymentId, string chargeId, decimal? amount = null)
        {
            return amount.HasValue ? await GetHeidelpay().CancelChargeAsync(paymentId, chargeId, amount.Value) :
                await GetHeidelpay().CancelChargeAsync(paymentId, chargeId);
        }

        #endregion

        #region Customer

        public async Task<string> GetUnzerCustomerAsync(string id)
        {
            return (await GetHeidelpay().FetchCustomerAsync(id))?.Id;
        }

        public async Task<string> CreateUnzerCustomerAsync(string firstName, string lastName, string email)
        {
            var customer = new Customer
            {
                Firstname = firstName,
                Lastname = lastName,
                Email = email
            };

            return (await GetHeidelpay().CreateCustomerAsync(customer))?.Id;
        }

        public async Task<string> UpdateUnzerCustomerAsync(string id, string firstName, string lastName, string email)
        {
            var customer = new Customer
            {
                Id = id,
                Firstname = firstName,
                Lastname = lastName,
                Email = email
            };

            return (await GetHeidelpay().UpdateCustomerAsync(customer))?.Id;
        }

        #endregion

        #region Card

        public async Task<Authorization> CreateAuthorizationForCardAsync(string resourceId, Guid orderGuid, string unzerCustomerId, decimal orderTotal, string currencyCode)
        {
            var card = await GetHeidelpay().FetchPaymentTypeAsync<Card>(resourceId.ToString());

            var initial = new Authorization(card)
            {
                OrderId = orderGuid.ToString(),
                CustomerId = unzerCustomerId,
                Amount = orderTotal,
                Currency = currencyCode,
                ReturnUrl = new Uri($"{_webHelper.GetStoreLocation()}" + UnzerPaymentDefaults.RedirectionUrlLastPart)
            };

            return await GetHeidelpay().AuthorizeAsync(initial);
        }

        #endregion

        #region Paypal

        public async Task<Authorization> CreatePaypalAuthorizationAsync(string resourceId, Guid orderGuid, string unzerCustomerId, decimal orderTotal, string currencyCode)
        {
            var paypal = await GetHeidelpay().FetchPaymentTypeAsync<Paypal>(resourceId.ToString());

            var initial = new Authorization(paypal)
            {
                OrderId = orderGuid.ToString(),
                CustomerId = unzerCustomerId,
                Amount = orderTotal,
                Currency = currencyCode,
                ReturnUrl = new Uri($"{_webHelper.GetStoreLocation()}" + UnzerPaymentDefaults.RedirectionUrlLastPart)
            };

            return await GetHeidelpay().AuthorizeAsync(initial);
        }

        #endregion Paypal

        #region Sofort

        public async Task<Charge> CreateChargeForSofortAsync(string resourceId, Guid orderGuid, string unzerCustomerId, decimal orderTotal, string currencyCode)
        {
            var sofort = await GetHeidelpay().FetchPaymentTypeAsync<Sofort>(resourceId.ToString());

            var initial = new Charge(sofort)
            {
                OrderId = orderGuid.ToString(),
                CustomerId = unzerCustomerId,
                Amount = orderTotal,
                Currency = currencyCode,
                ReturnUrl = new Uri($"{_webHelper.GetStoreLocation()}" + UnzerPaymentDefaults.RedirectionUrlLastPart)
            };

            return await GetHeidelpay().ChargeAsync(initial);
        }

        #endregion

        #region EPS

        public async Task<Charge> CreateChargeForEPSAsync(string resourceId, Guid orderGuid, string unzerCustomerId, decimal orderTotal, string currencyCode, string epsBic)
        {
            var eps = await GetHeidelpay().FetchPaymentTypeAsync<Eps>(resourceId.ToString());
            eps.Bic = epsBic;

            var initial = new Charge(eps)
            {
                OrderId = orderGuid.ToString(),
                CustomerId = unzerCustomerId,
                Amount = orderTotal,
                Currency = currencyCode,
                ReturnUrl = new Uri($"{_webHelper.GetStoreLocation()}" + UnzerPaymentDefaults.RedirectionUrlLastPart)
            };

            return await GetHeidelpay().ChargeAsync(initial);
        }

        #endregion EPS
    }
}
