using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Shipping.Redx.Areas.Admin.Models
{
    public record RedxAreaModel : BaseNopEntityModel
    {
        public RedxAreaModel()
        {
            AvailableStateProvinces = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.NopStation.Redx.RedxAreas.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Redx.RedxAreas.Fields.RedxArea")]
        public int RedxAreaId { get; set; }
        
        [NopResourceDisplayName("Admin.NopStation.Redx.RedxAreas.Fields.PostCode")]
        public string PostCode { get; set; }
        
        [NopResourceDisplayName("Admin.NopStation.Redx.RedxAreas.Fields.DistrictName")]
        public string DistrictName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Redx.RedxAreas.Fields.StateProvince")]
        public int? StateProvinceId { get; set; }

        public IList<SelectListItem> AvailableStateProvinces { get; set; }
    }
}