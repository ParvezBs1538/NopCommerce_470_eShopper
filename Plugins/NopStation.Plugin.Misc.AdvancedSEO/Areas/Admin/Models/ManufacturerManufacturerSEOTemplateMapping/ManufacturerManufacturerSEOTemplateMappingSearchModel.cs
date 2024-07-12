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
    public partial record ManufacturerManufacturerSEOTemplateMappingSearchModel : BaseSearchModel
    {

        public int ManufacturerSEOTemplateId { get; set; }
    }
}
