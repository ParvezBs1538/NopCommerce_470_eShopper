using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Misc.AdvancedSEO.Areas.Admin.Models.ManufacturerToMap
{
    public partial record ManufacturerToMapModel : BaseNopEntityModel
    {
        public ManufacturerToMapModel(Manufacturer manufacturer)
        {
            Id= manufacturer.Id;
            ManufacturerName = manufacturer.Name;
        }

        [NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerToMap.Fields.ManufacturerName")]
        public string ManufacturerName { get; set; }

        //[NopResourceDisplayName("Admin.NopStation.Plugin.Misc.AdvancedSEO.ManufacturerToMap.Fields.ManufacturerBreadCrumb")]
        //public string ManufacturerBreadCrumb { get; set; }

    }
}
