using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core;

namespace NopStation.Plugin.Misc.AdvancedSEO.Domains
{
    public class CategoryCategorySEOTemplateMapping : BaseEntity
    {
        public int CategoryId { get; set; }

        public int CategorySEOTemplateId { get; set; }
    }
}
