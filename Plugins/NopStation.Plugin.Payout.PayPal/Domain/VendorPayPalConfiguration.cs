using Nop.Core;

namespace NopStation.Plugin.Payout.PayPal.Domain
{
    public class VendorPayPalConfiguration : BaseEntity
    {
        public int VendorId { get; set; }
        public string PayPalEmail { get; set; }
    }
}
