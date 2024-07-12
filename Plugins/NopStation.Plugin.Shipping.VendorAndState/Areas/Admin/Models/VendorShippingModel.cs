using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Shipping.VendorAndState.Areas.Admin.Models
{
    public record VendorShippingModel : BaseNopEntityModel
    {
        public VendorShippingModel()
        {
            AvailableShippingMethods = new List<SelectListItem>();
            VendorStateProvinceShippingSearchModel = new VendorStateProvinceShippingSearchModel();
        }

        [NopResourceDisplayName("Admin.NopStation.ShippingByVendor.VendorShippings.Fields.Vendor")]
        public string VendorName { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ShippingByVendor.VendorShippings.Fields.Vendor")]
        public int VendorId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ShippingByVendor.VendorShippings.Fields.ShippingMethod")]
        public string ShippingMethod { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ShippingByVendor.VendorShippings.Fields.ShippingMethod")]
        public int ShippingMethodId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ShippingByVendor.VendorShippings.Fields.HideShippingMethod")]
        public bool HideShippingMethod { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ShippingByVendor.VendorShippings.Fields.DefaultShippingCharge")]
        public decimal DefaultShippingCharge { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ShippingByVendor.VendorShippings.Fields.SellerSideDelivery")]
        public bool SellerSideDelivery { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ShippingByVendor.VendorShippings.Fields.EnableFreeShippingOverAmountX")]
        public bool EnableFreeShippingOverAmountX { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ShippingByVendor.VendorShippings.Fields.AmountX")]
        public decimal AmountX { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ShippingByVendor.VendorShippings.Fields.WithDiscounts")]
        public bool WithDiscounts { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ShippingByVendor.VendorShippings.Fields.TransitDays")]
        public int? TransitDays { get; set; }
        public IList<SelectListItem> AvailableShippingMethods { get; set; }

        public VendorStateProvinceShippingSearchModel VendorStateProvinceShippingSearchModel { get; set; }
    }
}
