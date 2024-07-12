using System;
using System.Threading.Tasks;
using Heidelpay.Payment;

namespace NopStation.Plugin.Payments.Unzer.Services
{
    public interface IUnzerHelperService
    {
        #region Card and Common

        Task<Authorization> CreateAuthorizationForCardAsync(string resourceId, Guid orderGuid, string customerId, decimal orderTotal, string currencyCode);

        Task<Charge> CreateChargeByAuthorizationAsync(Authorization authorization);

        Task<Payment> FetchPaymentByPaymentIdAsync(string paymentId);

        Task<Charge> FetchChargeByPaymentIdAndChargeIdAsync(string paymentId, string chargeId);

        Task<Cancel> CancelChargeByPaymentIdAndChargeIdAsync(string paymentId, string chargeId, decimal? amount = null);

        Task<Cancel> CancelAuthorizationByPaymentIdAsync(string paymentId);

        #endregion

        #region Customer

        Task<string> GetUnzerCustomerAsync(string id);

        Task<string> CreateUnzerCustomerAsync(string firstName, string lastName, string email);

        Task<string> UpdateUnzerCustomerAsync(string id, string firstName, string lastName, string email);

        #endregion

        #region Paypal

        Task<Authorization> CreatePaypalAuthorizationAsync(string resourceId, Guid orderGuid, string customerId, decimal orderTotal, string currencyCode);

        #endregion

        #region EPS

        Task<Charge> CreateChargeForEPSAsync(string resourceId, Guid orderGuid, string customerId, decimal orderTotal, string currencyCode, string epsBic);

        #endregion

        #region Sofort

        Task<Charge> CreateChargeForSofortAsync(string resourceId, Guid orderGuid, string customerId, decimal orderTotal, string currencyCode);

        #endregion
    }
}
