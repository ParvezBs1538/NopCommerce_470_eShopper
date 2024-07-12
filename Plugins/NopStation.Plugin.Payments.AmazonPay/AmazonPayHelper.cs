using System;
using Amazon.Pay.API.Types;

namespace NopStation.Plugin.Payments.AmazonPay
{
    public static class AmazonPayHelper
    {
        public static Currency GetCurrencyEnum(string currencyCode)
        {
            if (string.IsNullOrWhiteSpace(currencyCode))
                throw new ArgumentNullException($"Currency not supported {nameof(currencyCode)}");

            if (Enum.TryParse(currencyCode, true, out Currency result))
                return result;

            throw new ArgumentNullException($"Currency not supported {nameof(currencyCode)}");
        }

        public static bool ValidateCurrency(this Nop.Core.Domain.Directory.Currency currency)
        {
            if (currency == null)
                return false;

            return Enum.TryParse(currency.CurrencyCode, true, out Currency _);
        }
    }
}
