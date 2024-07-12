using Nop.Core.Caching;

namespace NopStation.Plugin.Payments.MPay24.Services.Cache
{
    public class MPay24CacheDefaults
    {
        public static CacheKey PaymentOptionByShortNameKey => new CacheKey("Nopstation.payments.mpay24.paymentoptions.byshortname.{0}", PaymentOptionPrefix);
        public static CacheKey PaymentOptionAllKey => new CacheKey("Nopstation.payments.mpay24.paymentoptions.all.{0}-{1}-{2}-{3}-{4}-{5}", PaymentOptionPrefix);
        public static string PaymentOptionPrefix => "Nopstation.payments.mpay24.paymentoptions.";
    }
}