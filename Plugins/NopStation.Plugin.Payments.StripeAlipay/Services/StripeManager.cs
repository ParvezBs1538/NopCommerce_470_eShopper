using System;
using Nop.Services.Directory;

namespace NopStation.Plugin.Payments.StripeAlipay.Services
{
    public class StripeManager
    {
        #region Fields

        private const int RATE = 100;
        private readonly ICurrencyService _currencyService;

        #endregion

        #region Ctor

        public StripeManager(ICurrencyService currencyService)
        {
            _currencyService = currencyService;
        }

        #endregion

        #region Methods

        public long ConvertCurrencyToLong(decimal total, decimal currencyRate)
        {
            return Convert.ToInt64(_currencyService.ConvertCurrency(total, currencyRate) * RATE);
        }

        public decimal ConvertCurrencyFromLong(decimal total, decimal currencyRate)
        {
            return _currencyService.ConvertCurrency(total / RATE, 1 / currencyRate);
        }

        #endregion
    }
}
