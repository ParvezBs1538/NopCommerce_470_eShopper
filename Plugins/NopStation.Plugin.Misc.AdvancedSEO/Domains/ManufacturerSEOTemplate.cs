using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NopStation.Plugin.Misc.AdvancedSEO.Domains
{
    public class ManufacturerSEOTemplate : SEOTemplateBaseEntity
    {
        public bool IncludeProductNamesOnKeyword { get; set; }

        public int MaxNumberOfProductToInclude { get; set; }
    }
}
