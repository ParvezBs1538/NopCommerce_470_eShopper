using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Payments.Afterpay.Areas.Admin.Models
{
    public class ConfigurationModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Afterpay.Fields.UseSandbox")]
        public bool UseSandbox { get; set; }
        public bool UseSandbox_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Afterpay.Fields.Debug")]
        public bool Debug { get; set; }
        public bool Debug_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Afterpay.Fields.MerchantId")]
        public string MerchantId { get; set; }
        public bool MerchantId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Afterpay.Fields.MerchantKey")]
        public string MerchantKey { get; set; }
        public bool MerchantKey_OverrideForStore { get; set; }


        [NopResourceDisplayName("Admin.NopStation.Afterpay.Fields.MinimumAmount")]
        public int MinimumAmount { get; set; }
        public bool MinimumAmount_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Afterpay.Fields.MaximumAmount")]
        public int MaximumAmount { get; set; }
        public bool MaximumAmount_OverrideForStore { get; set; }
    }
}
