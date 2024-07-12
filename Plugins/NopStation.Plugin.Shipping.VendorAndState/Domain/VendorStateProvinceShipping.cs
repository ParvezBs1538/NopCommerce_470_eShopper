using Nop.Core;

namespace NopStation.Plugin.Shipping.VendorAndState.Domain
{
    public class VendorStateProvinceShipping : BaseEntity
    {
        public int ShippingMethodId { get; set; }

        public int StateProvinceId { get; set; }

        public int VendorId { get; set; }

        public bool HideShippingMethod { get; set; }

        public decimal ShippingCharge { get; set; }

        public bool EnableFreeShippingOverAmountX { get; set; }

        public decimal AmountX { get; set; }

        public bool WithDiscounts { get; set; }

        public int? TransitDays { get; set; }
    }
}
