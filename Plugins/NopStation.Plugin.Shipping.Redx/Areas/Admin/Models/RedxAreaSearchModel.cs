using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Shipping.Redx.Areas.Admin.Models
{
    public record RedxAreaSearchModel : BaseSearchModel
    {
        public RedxAreaSearchModel()
        {
            AvailableStateProvinces = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.NopStation.Redx.RedxAreas.List.SearchDisctrictName")]
        public string SearchDisctrictName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Redx.RedxAreas.List.SearchStateProvince")]
        public int SearchStateProvinceId { get; set; }

        public IList<SelectListItem> AvailableStateProvinces { get; set; }
    }
}