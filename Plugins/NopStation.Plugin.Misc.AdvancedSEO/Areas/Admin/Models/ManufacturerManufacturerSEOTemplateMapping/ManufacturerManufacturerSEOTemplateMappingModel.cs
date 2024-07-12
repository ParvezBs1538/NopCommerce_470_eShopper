using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Models.ManufacturerManufacturerSEOTemplateMapping
{
    public partial record ManufacturerManufacturerSEOTemplateMappingModel : BaseNopEntityModel
    {

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerManufacturerSEOTemplateMapping.Fields.Manufacturer")]
        public string ManufacturerName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerManufacturerSEOTemplateMapping.Fields.Manufacturer")]
        public int ManufacturerId { get; set; }

        public int ManufacturerSEOTemplateId { get; set; }

        
    }
}
