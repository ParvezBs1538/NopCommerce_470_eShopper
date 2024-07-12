using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Payments.Nagad.Areas.Admin.Models
{
    public class ConfigurationModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Nagad.Configuration.Fields.Description")]
        public string Description { get; set; }
        public bool Description_OverrideForStore { get; set; }
        [NopResourceDisplayName("Admin.NopStation.Nagad.Configuration.Fields.UseSandbox")]
        public bool UseSandbox { get; set; }
        public bool UseSandbox_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Nagad.Configuration.Fields.MerchantId")]
        public string MerchantId { get; set; }
        public bool MerchantId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Nagad.Configuration.Fields.NPGPublicKey")]
        public string NPGPublicKey { get; set; }
        public bool NPGPublicKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Nagad.Configuration.Fields.MSPrivateKey")]
        public string MSPrivateKey { get; set; }
        public bool MSPrivateKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Nagad.Configuration.Fields.AdditionalFee")]
        public decimal AdditionalFee { get; set; }
        public bool AdditionalFee_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Nagad.Configuration.Fields.AdditionalFeePercentage")]
        public bool AdditionalFeePercentage { get; set; }
        public bool AdditionalFeePercentage_OverrideForStore { get; set; }
    }
}
