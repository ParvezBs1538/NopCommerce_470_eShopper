using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Payments.POLiPay.Areas.Admin.Models
{
    public record ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PoliPay.Configuration.Fields.UseSandbox")]
        public bool UseSandbox { get; set; }
        public bool UseSandbox_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PoliPay.Configuration.Fields.MerchantCode")]
        public string MerchantCode { get; set; }
        public bool MerchantCode_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PoliPay.Configuration.Fields.AuthCode")]
        public string AuthCode { get; set; }
        public bool AuthCode_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PoliPay.Configuration.Fields.PoliPayURL")]
        public string PoliPayURL { get; set; }
        public bool PoliPayURL_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PoliPay.Configuration.Fields.AdditionalFee")]
        public decimal AdditionalFee { get; set; }
        public bool AdditionalFee_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.PoliPay.Configuration.Fields.AdditionalFeePercentage")]
        public bool AdditionalFeePercentage { get; set; }
        public bool AdditionalFeePercentage_OverrideForStore { get; set; }
    }
}