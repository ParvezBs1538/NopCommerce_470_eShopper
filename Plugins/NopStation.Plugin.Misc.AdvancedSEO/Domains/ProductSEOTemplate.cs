using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NopStation.Plugin.Misc.AdvancedSEO.Domains
{
    public class ProductSEOTemplate : SEOTemplateBaseEntity
    {
        public bool IncludeProductTagsOnKeyword { get; set; }

        public bool IncludeCategoryNamesOnKeyword { get; set; }

        public bool IncludeManufacturerNamesOnKeyword { get; set; }

        public bool IncludeVendorNamesOnKeyword { get; set; }

    }
}
