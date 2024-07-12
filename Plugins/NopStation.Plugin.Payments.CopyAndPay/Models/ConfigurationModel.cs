using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Payments.CopyAndPay.Models
{
    public record ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CopyAndPay.Configuration.Fields.AdditionalFee")]
        public decimal AdditionalFee { get; set; }
        public bool AdditionalFee_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CopyAndPay.Configuration.Fields.AdditionalFeePercentage")]
        public bool AdditionalFeePercentage { get; set; }
        public bool AdditionalFeePercentage_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CopyAndPay.Configuration.Fields.EntityId")]
        public string EntityId { get; set; }
        public bool EntityId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CopyAndPay.Configuration.Fields.MadaEntityId")]
        public string MadaEntityId { get; set; }
        public bool MadaEntityId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CopyAndPay.Configuration.Fields.TestMode")]
        public string TestMode { get; set; }
        public bool TestMode_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CopyAndPay.Configuration.Fields.AuthorizationKey")]
        public string AuthorizationKey { get; set; }
        public bool AuthorizationKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CopyAndPay.Configuration.Fields.APIUrl")]
        public string APIUrl { get; set; }
        public bool APIUrl_OverrideForStore { get; set; }
    }
}