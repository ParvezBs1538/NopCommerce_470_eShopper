using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;

namespace NopStation.Plugin.Shipping.VendorAndState.Areas.Admin.Models
{
    public partial record ConfigurationModel : BaseNopModel, ISettingsModel
    {
        [NopResourceDisplayName("Admin.NopStation.ShippingByVendor.Configuration.Fields.EnablePlugin")]
        public bool EnablePlugin { get; set; }
        public bool EnablePlugin_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ShippingByVendor.Configuration.Fields.ShippingCharge")]
        public decimal ShippingCharge { get; set; }
        public bool ShippingCharge_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ShippingByVendor.Configuration.Fields.EnableFreeShippingOverAmountX")]
        public bool EnableFreeShippingOverAmountX { get; set; }
        public bool EnableFreeShippingOverAmountX_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ShippingByVendor.Configuration.Fields.AmountX")]
        public decimal AmountX { get; set; }
        public bool AmountX_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ShippingByVendor.Configuration.Fields.WithDiscounts")]
        public bool WithDiscounts { get; set; }
        public bool WithDiscounts_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.ShippingByVendor.Configuration.Fields.TransitDays")]
        public int? TransitDays { get; set; }
        public bool TransitDays_OverrideForStore { get; set; }

        public int ActiveStoreScopeConfiguration { get; set; }
    }
}
