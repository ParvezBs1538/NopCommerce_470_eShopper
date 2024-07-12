using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Payout.Stripe.Areas.Admin.Models
{
    public record ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("NopStation.Plugin.Payout.Stripe.Active")]
        public bool Active { get; set; }
        public bool Active_OverrideForStore { get; set; }

        [NopResourceDisplayName("NopStation.Plugin.Payout.Stripe.UseSandbox")]
        public bool UseSandbox { get; set; }
        public bool UseSandbox_OverrideForStore { get; set; }

        [NopResourceDisplayName("NopStation.Plugin.Payout.Stripe.SecretKey")]
        public string SecretKey { get; set; }
        public bool SecretKey_OverrideForStore { get; set; }
    }
}