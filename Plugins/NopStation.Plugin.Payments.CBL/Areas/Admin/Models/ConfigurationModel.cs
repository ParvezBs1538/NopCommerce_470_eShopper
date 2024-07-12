using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Payments.CBL.Areas.Admin.Models
{
    public class ConfigurationModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CBL.Fields.UseSandbox")]
        public bool UseSandbox { get; set; }
        public bool UseSandbox_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CBL.Fields.Debug")]
        public bool Debug { get; set; }
        public bool Debug_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CBL.Fields.MerchantId")]
        public string MerchantId { get; set; }
        public bool MerchantId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CBL.Fields.MerchantUsername")]
        public string MerchantUsername { get; set; }
        public bool MerchantUsername_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.CBL.Fields.MerchantPassword")]
        public string MerchantPassword { get; set; }
        public bool MerchantPassword_OverrideForStore { get; set; }
    }
}
