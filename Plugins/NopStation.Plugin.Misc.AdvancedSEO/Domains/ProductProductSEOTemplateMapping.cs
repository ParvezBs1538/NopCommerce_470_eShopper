using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core;

namespace NopStation.Plugin.Misc.AdvancedSEO.Domains
{
    public class ProductProductSEOTemplateMapping : BaseEntity
    {
        public int ProductId { get; set; }
        public int ProductSEOTemplateId { get; set; }
    }
}
