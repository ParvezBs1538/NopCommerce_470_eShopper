using System.Collections.Generic;
using Nop.Core;
using Nop.Core.Caching;

namespace NopStation.Plugin.Tax.TaxJar
{
    public class TaxJarDefaults
    {
        /// <summary>
        /// Gets the taxjar tax provider system name
        /// </summary>
        public static string SystemName => "NopStation.Plugin.Tax.TaxJar";

        /// <summary>
        /// Gets the taxjar tax provider output dictory name
        /// </summary>
        public static string PluginOutputDir => "NopStation.Plugin.Tax.TaxJar";

        /// <summary>
        /// Gets the taxjar tax provider connector name
        /// </summary>
        public static string ApplicationName => "nopCommerce-TaxJarRateProvider|a0o33000004BoPM";

        /// <summary>
        /// Gets the taxjar tax provider version (used a nopCommerce version here)
        /// </summary>
        public static string ApplicationVersion => $"v{NopVersion.CURRENT_VERSION}";

        /// <summary>
        /// Gets the configuration route name
        /// </summary>
        public static string ConfigurationRouteName => "Plugin.NopStation.TaxJar.Configure";
        public static string CategoryRouteName => "Plugin.NopStation.TaxJar.Category";
        public static string TaxjarCategory => "Plugin.NopStation.TaxJar.Category";
        public static CacheKey TaxTotalCacheKey => new CacheKey("NopStation.TaxJar.TaxRate.{0}-{1}-{2}-{3}-{4}", TaxRateCacheKeyByCustomerPrefix);

        public static int TaxTotalCacheTime => 60;

        /// <summary>
        /// Gets the key for caching tax rate
        /// </summary>
        /// <remarks>
        /// {0} - Customer id
        /// {1} - Tax category id
        /// {2} - Address
        /// {3} - City
        /// {4} - State or province identifier
        /// {5} - Country identifier
        /// {6} - Zip postal code
        /// </remarks>
        public static CacheKey TaxRateCacheKey => new("NopStation.TaxJar.TaxRate.{0}-{1}-{2}-{3}-{4}-{5}-{6}", TaxRateCacheKeyByCustomerPrefix);

        /// <summary>
        /// Gets a key pattern to clear cache
        /// </summary>
        /// <remarks>
        /// {0} : Customer id
        /// </remarks>
        public static string TaxRateCacheKeyByCustomerPrefix => "NopStation.TaxJar.TaxRate.{0}-";

        public static string TaxjarSandBoxApiUrl => "https://api.sandbox.taxjar.com";
        public static string TaxjarApiVersionHeaderKey => "x-api-version";

        public static Dictionary<int, string> TaxJarApiVersions = new Dictionary<int, string>()
        {
            { 1, "2012-01-01" },
            { 2, "2020-08-07" },
            { 3, "2022-01-24"}
        };

        public static int TaxRateCacheTime => 480;

        public static string TaxTotalCacheKeyPrefix => "NopStation.TaxJar.TaxRate.{0}-";

        public static string TaxJarCacheKeyPrefix => "NopStation.TaxJar";
    }
}
