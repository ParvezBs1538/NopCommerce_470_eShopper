using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Models.ProductProductSEOTemplateMapping
{
    public partial record ProductProductSEOTemplateMappingModel : BaseNopEntityModel
    {

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductProductSEOTemplateMapping.Fields.Product")]
        public string ProductName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductProductSEOTemplateMapping.Fields.Product")]
        public int ProductId { get; set; }

        public int ProductSEOTemplateId { get; set; }

        
    }
}
