using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Caching;

namespace NopStation.Plugin.Misc.AdvancedSEO
{
    public class AdvancedSEOPluginDefaults
    {
        public static string PluginSystemName => "NopStation.Plugin.Misc.AdvancedSEO";

        public static string PluginOutputDir => "NopStation.Plugin.Misc.AdvancedSEO";


        public static CacheKey CategorySEOTemplateCacheKey => new("NopStation.AdvancedSEO.SeoTemplate.CategorySeoTemplate.{0}-{1}-{2}", CategorySeoTemplateCacheKeyPrefix);

        public static string CategorySeoTemplateCacheKeyPrefix => "NopStation.AdvancedSEO.SeoTemplate.CategorySeoTemplate.";

        public static CacheKey ManufacturerSEOTemplateCacheKey => new("NopStation.AdvancedSEO.SeoTemplate.ManufacturerSeoTemplate.{0}-{1}-{2}", ManufacturerSeoTemplateCacheKeyPrefix);

        public static string ManufacturerSeoTemplateCacheKeyPrefix => "NopStation.AdvancedSEO.SeoTemplate.ManufacturerSeoTemplate.";

        public static CacheKey ProductSEOTemplateCacheKey => new("NopStation.AdvancedSEO.SeoTemplate.ProductSeoTemplate.{0}-{1}-{2}", ProductSeoTemplateCacheKeyPrefix);

        public static string ProductSeoTemplateCacheKeyPrefix => "NopStation.AdvancedSEO.SeoTemplate.ProductSeoTemplate.";

        public static string AdvancedSEOPluginPrefix => "NopStation.AdvancedSEO.SeoTemplate.ProductSeoTemplate.";


    }
}
