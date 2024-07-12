using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Models.CategoryCategorySEOTemplateMapping
{
    public partial record CategoryCategorySEOTemplateMappingModel : BaseNopEntityModel
    {

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.CategoryCategorySEOTemplateMapping.Fields.Category")]
        public string CategoryName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.CategoryCategorySEOTemplateMapping.Fields.Category")]
        public int CategoryId { get; set; }

        public int CategorySEOTemplateId { get; set; }

        
    }
}
