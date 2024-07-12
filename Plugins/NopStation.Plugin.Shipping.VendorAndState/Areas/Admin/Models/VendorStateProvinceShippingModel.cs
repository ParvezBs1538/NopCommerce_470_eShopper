using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Shipping.VendorAndState.Areas.Admin.Models
{
    public record VendorStateProvinceShippingModel : BaseNopEntityModel
    {
        public string ComplexId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ShippingByVendor.VendorStateProvinceShippings.Fields.ShippingMethod")]
        public int ShippingMethodId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ShippingByVendor.VendorStateProvinceShippings.Fields.StateProvince")]
        public int StateProvinceId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ShippingByVendor.VendorStateProvinceShippings.Fields.StateProvince")]
        public string StateProvince { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ShippingByVendor.VendorStateProvinceShippings.Fields.Vendor")]
        public int VendorId { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ShippingByVendor.VendorStateProvinceShippings.Fields.HideShippingMethod")]
        public bool HideShippingMethod { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ShippingByVendor.VendorStateProvinceShippings.Fields.ShippingCharge")]
        public decimal ShippingCharge { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ShippingByVendor.VendorStateProvinceShippings.Fields.EnableFreeShippingOverAmountX")]
        public bool EnableFreeShippingOverAmountX { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ShippingByVendor.VendorStateProvinceShippings.Fields.AmountX")]
        public decimal AmountX { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ShippingByVendor.VendorStateProvinceShippings.Fields.WithDiscounts")]
        public bool WithDiscounts { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ShippingByVendor.VendorStateProvinceShippings.Fields.TransitDays")]
        public int? TransitDays { get; set; }

        public bool IsSet { get; set; }
    }
}
