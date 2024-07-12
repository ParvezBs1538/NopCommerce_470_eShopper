using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Payments.Quickstream.Areas.Admin.Models
{
    public record ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Admin.NopStation.QuickStream.Configuration.Fields.UseSandbox")]
        public bool UseSandbox { get; set; }
        public bool UseSandbox_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.QuickStream.Configuration.Fields.PublishableApiKey")]
        public string PublishableApiKey { get; set; }
        public bool PublishableApiKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.QuickStream.Configuration.Fields.SecretApiKey")]
        public string SecretApiKey { get; set; }
        public bool SecretApiKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.QuickStream.Configuration.Fields.SupplierBusinessCode")]
        public string SupplierBusinessCode { get; set; }
        public bool SupplierBusinessCode_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.QuickStream.Configuration.Fields.CommunityCode")]
        public string CommunityCode { get; set; }
        public bool CommunityCode_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.QuickStream.Configuration.Fields.IpAddress")]
        public string IpAddress { get; set; }
        public bool IpAddress_OverrideForStore { get; set; }
    }
}
