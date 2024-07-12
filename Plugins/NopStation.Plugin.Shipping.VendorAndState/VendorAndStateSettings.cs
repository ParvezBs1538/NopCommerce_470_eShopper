using Nop.Core.Configuration;

namespace NopStation.Plugin.Shipping.VendorAndState
{
    public class VendorAndStateSettings : ISettings
    {
        public bool EnablePlugin { get; set; }

        public decimal ShippingCharge { get; set; }

        public bool EnableFreeShippingOverAmountX { get; set; }

        public decimal AmountX { get; set; }

        public bool WithDiscounts { get; set; }

        public int? TransitDays { get; set; }
    }
}
