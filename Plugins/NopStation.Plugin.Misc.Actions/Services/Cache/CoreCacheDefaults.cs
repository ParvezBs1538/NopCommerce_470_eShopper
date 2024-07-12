using Nop.Core.Caching;

namespace NopStation.Plugin.Misc.Core.Services.Cache;

public class CoreCacheDefaults
{
    public static CacheKey LicenseKey => new CacheKey("Nopstation.core.keys.all-{0}", LicensePrefix);
    public static string LicensePrefix => "Nopstation.core.keys.";
}