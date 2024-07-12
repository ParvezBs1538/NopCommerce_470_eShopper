using Nop.Core;

namespace NopStation.Plugin.Shipping.VendorAndState.Domain
{
    public class VendorShipping : BaseEntity
    {
        public int ShippingMethodId { get; set; }

        public int VendorId { get; set; }

        public bool HideShippingMethod { get; set; }

        public decimal DefaultShippingCharge { get; set; }

        public bool SellerSideDelivery { get; set; }

        public bool EnableFreeShippingOverAmountX { get; set; }

        public decimal AmountX { get; set; }

        public bool WithDiscounts { get; set; }

        public int? TransitDays { get; set; }
    }
}
