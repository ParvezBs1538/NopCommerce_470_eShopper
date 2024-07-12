using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Payout.Stripe.Areas.Admin.Models
{
    public record VendorStripeConfigurationModel : BaseNopModel
    {
        public int VendorId { get; set; }

        [NopResourceDisplayName("NopStation.Plugin.Payout.Stripe.Configuration.Fields.AccountId")]
        public string AccountId { get; set; }
    }
}
