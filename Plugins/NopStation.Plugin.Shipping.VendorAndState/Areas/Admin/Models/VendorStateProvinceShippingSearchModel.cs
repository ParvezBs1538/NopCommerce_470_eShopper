using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Shipping.VendorAndState.Areas.Admin.Models
{
    public record VendorStateProvinceShippingSearchModel : BaseSearchModel
    {
        public VendorStateProvinceShippingSearchModel()
        {
            AvailableCountries = new List<SelectListItem>();
        }

        public int SearchCountryId { get; set; }
        public int VendorId { get; set; }
        public int ShippingMethodId { get; set; }
        public IList<SelectListItem> AvailableCountries { get; set; }
    }
}
