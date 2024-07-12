using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Payments.OABIPayment.Models
{
    public record ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Payments.OABIPayment.TranPortaId")]
        public string TranPortalId { get; set; }
        public bool TranPortalId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Payments.OABIPayment.ResourceKey")]
        public string ResourceKey { get; set; }
        public bool ResourceKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Payments.OABIPayment.TranPortaPassword")]
        public string TranPortaPassword { get; set; }
        public bool TranPortaPassword_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Payments.OABIPayment.AdditionalFee")]
        public decimal AdditionalFee { get; set; }
        public bool AdditionalFee_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Payments.OABIPayment.AdditionalFeeInPercentage")]
        public bool AdditionalFeeInPercentage { get; set; }
        public bool AdditionalFeeInPercentage_OverrideForStore { get; set; }
    }
}
