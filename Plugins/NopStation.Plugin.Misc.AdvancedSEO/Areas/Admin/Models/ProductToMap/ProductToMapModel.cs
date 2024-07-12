using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Models.ProductToMap
{
    public partial record ProductToMapModel : BaseNopEntityModel
    {
        public ProductToMapModel(Product product)
        {
            Id= product.Id;
            ProductName = product.Name;
        }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductToMap.Fields.ProductName")]
        public string ProductName { get; set; }

        //[NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.ProductToMap.Fields.ProductBreadCrumb")]
        //public string ProductBreadCrumb { get; set; }

    }
}
