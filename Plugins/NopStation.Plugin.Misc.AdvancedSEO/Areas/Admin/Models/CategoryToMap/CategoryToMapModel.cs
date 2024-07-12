using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Models.CategoryToMap
{
    public partial record CategoryToMapModel : BaseNopEntityModel
    {
        public CategoryToMapModel(Category category)
        {
            Id= category.Id;
            CategoryName = category.Name;
        }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.CategoryToMap.Fields.CategoryName")]
        public string CategoryName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.CategoryToMap.Fields.CategoryBreadCrumb")]
        public string CategoryBreadCrumb { get; set; }

    }
}
