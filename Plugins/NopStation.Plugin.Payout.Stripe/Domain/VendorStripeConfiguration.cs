using Nop.Core;

namespace NopStation.Plugin.Payout.Stripe.Domain
{
    public class VendorStripeConfiguration : BaseEntity
    {
        public int VendorId { get; set; }
        public string AccountId { get; set; }
    }
}
