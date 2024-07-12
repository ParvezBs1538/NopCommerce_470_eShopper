using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Common;

namespace NopStation.Plugin.Misc.AdvancedSEO.Domains
{
    public class CategorySEOTemplate : SEOTemplateBaseEntity
    {
        public bool IncludeProductNamesOnKeyword  { get; set; }

        public int MaxNumberOfProductToInclude { get; set; }
    }
}
