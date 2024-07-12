using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Shipping.VendorAndState.Areas.Admin.Models
{
    public record VendorShippingSearchModel : BaseSearchModel
    {
        public VendorShippingSearchModel()
        {
            AvailableShippingMethods = new List<SelectListItem>();
            AvailableVendors = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.NopStation.ShippingByVendor.VendorShippings.List.SearchVendor")]
        public int SearchVendorId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ShippingByVendor.VendorShippings.List.SearchShippingMethod")]
        public int SearchShippingMethodId { get; set; }
        public IList<SelectListItem> AvailableShippingMethods { get; set; }
        public IList<SelectListItem> AvailableVendors { get; set; }
    }
}
