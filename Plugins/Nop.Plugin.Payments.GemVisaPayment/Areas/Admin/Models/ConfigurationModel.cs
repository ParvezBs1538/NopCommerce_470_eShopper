using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Payments.GemVisaPayment.Areas.Admin.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.Payments.GemVisa.Fields.UseSandbox")]
        public bool UseSandbox { get; set; }
        public bool UseSandbox_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.GemVisa.Fields.DebugMode")]
        public bool DebugMode { get; set; }
        public bool DebugMode_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.GemVisa.Fields.MerchantId")]
        public string MerchantId { get; set; }
        public bool MerchantId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.GemVisa.Fields.SecretKey")]
        public string SecretKey { get; set; }
        public bool SecretKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.GemVisa.Fields.MinimumAmount")]
        public int MinimumAmount { get; set; }
        public bool MinimumAmount_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.GemVisa.Fields.MaximumAmount")]
        public int MaximumAmount { get; set; }
        public bool MaximumAmount_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.GemVisa.Fields.ShowInfoInProductDetailsPage")]
        public bool ShowInfoInProductDetailsPage { get; set; }
        public bool ShowInfoInProductDetailsPage_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.GemVisa.Fields.InstallmentMonth")]
        public int InstallmentMonth { get; set; }
        public bool InstallmentMonth_OverrideForStore { get; set; }

    }
}