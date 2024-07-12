using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Data.Mapping;
using NopStation.Plugin.Misc.AdvancedSEO.Domains;

namespace NopStation.Plugin.Misc.AdvancedSEO.Data
{
    public class BaseNameCompatibility : INameCompatibility
    {
        public Dictionary<Type, string> TableNames => new Dictionary<Type, string>
        {
            { typeof(CategorySEOTemplate), "NS_AS_CategorySEOTemplate" },
            { typeof(CategoryCategorySEOTemplateMapping), "NS_AS_CategoryCategorySEOTemplateMap" },
            { typeof(ManufacturerSEOTemplate), "NS_AS_ManufacturerSEOTemplate" },
            { typeof(ManufacturerManufacturerSEOTemplateMapping), "NS_AS_Manuf_ManufacturerSEOTemplateMap" },
            { typeof(ProductSEOTemplate), "NS_AS_ProductSEOTemplate" },
            { typeof(ProductProductSEOTemplateMapping), "NS_AS_ProductProductSEOTemplateMap" }
        };

        public Dictionary<(Type, string), string> ColumnName => new Dictionary<(Type, string), string>
        {

        };

    }
}