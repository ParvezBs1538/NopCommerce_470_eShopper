using Nop.Core.Caching;

namespace NopStation.Plugin.Payments.Affirm.Services.Cache
{
    public class AffirmPaymentCacheDefaults
    {
        public static CacheKey AffirmTransactionsAllKey => new CacheKey("Nopstation.affirmpayment.transactions.all.{0}-{1}-{2}-{3}", AffirmTransactionPrefix);
        public static CacheKey AffirmTransactionByReferenceKey => new CacheKey("Nopstation.affirmpayment.transactions.byreference.{0}", AffirmTransactionPrefix);
        public static string AffirmTransactionPrefix => "Nopstation.affirmpayment.transactions.";
    }
}