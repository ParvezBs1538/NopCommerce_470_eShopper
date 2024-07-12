using Nop.Core.Caching;

namespace NopStation.Plugin.Widgets.DMS.Services.Cache
{
    public class DMSCacheDefaults
    {
        public static CacheKey StringResourceKey => new CacheKey("Nopstation.dms.stringresource-{0}", StringResourcePrefixCacheKey);
        public static string StringResourcePrefixCacheKey => "Nopstation.dms.stringresource";
    }
}
