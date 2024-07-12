using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core;

namespace NopStation.Plugin.Misc.AdvancedSEO.Domains
{
    public class ManufacturerManufacturerSEOTemplateMapping : BaseEntity
    {
        public int ManufacturerId { get; set; }

        public int ManufacturerSEOTemplateId { get; set; }
    }
}
