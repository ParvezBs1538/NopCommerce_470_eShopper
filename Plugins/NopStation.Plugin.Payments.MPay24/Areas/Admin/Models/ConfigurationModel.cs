using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Payments.MPay24.Areas.Admin.Models
{
    public record ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Admin.NopStation.MPay24.Configuration.Fields.Sandbox")]
        public bool Sandbox { get; set; }
        public bool Sandbox_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.MPay24.Configuration.Fields.SoapUsername")]
        public string SoapUsername { get; set; }
        public bool SoapUsername_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.MPay24.Configuration.Fields.SoapPassword")]
        public string SoapPassword { get; set; }
        public bool SoapPassword_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.MPay24.Configuration.Fields.AdditionalFeePercentage")]
        public bool AdditionalFeePercentage { get; set; }
        public bool AdditionalFeePercentage_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.MPay24.Configuration.Fields.AdditionalFee")]
        public decimal AdditionalFee { get; set; }
        public bool AdditionalFee_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.MPay24.Configuration.Fields.MerchantId")]
        public int MerchantId { get; set; }
        public bool MerchantId_OverrideForStore { get; set; }
    }
}
