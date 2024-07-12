using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Payout.PayPal.Areas.Admin.Models
{
    public record ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("NopStation.Plugin.Payout.PayPal.Active")]
        public bool Active { get; set; }
        public bool Active_OverrideForStore { get; set; }

        [NopResourceDisplayName("NopStation.Plugin.Payout.PayPal.UseSandBox")]
        public bool UseSandbox { get; set; }
        public bool UseSandbox_OverrideForStore { get; set; }

        [NopResourceDisplayName("NopStation.Plugin.Payout.PayPal.ClientId")]
        public string ClientId { get; set; }
        public bool ClientId_OverrideForStore { get; set; }

        [NopResourceDisplayName("NopStation.Plugin.Payout.PayPal.SecretKey")]
        public string SecretKey { get; set; }
        public bool SecretKey_OverrideForStore { get; set; }
    }
}