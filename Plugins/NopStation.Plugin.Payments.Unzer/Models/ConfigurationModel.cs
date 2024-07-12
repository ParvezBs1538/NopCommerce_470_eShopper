using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Payments.Unzer.Models
{
    public record ConfigurationModel : BaseNopModel
    {
        [NopResourceDisplayName("Admin.NopStation.Unzer.Configuration.Fields.Endpoint")]
        public string ApiEndpoint { get; set; }
        public bool ApiEndpoint_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Unzer.Configuration.Fields.ApiVersion")]
        public string ApiVersion { get; set; }
        public bool ApiVersion_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Unzer.Configuration.Fields.UseSandbox")]
        public bool UseSandbox { get; set; }
        public bool UseSandbox_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Unzer.Configuration.Fields.IsSofortActive")]
        public bool IsSofortActive { get; set; }
        public bool IsSofortActive_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Unzer.Configuration.Fields.TransactionMode")]
        public int TransactionModeId { get; set; }
        public bool TransactionModeId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Unzer.Configuration.Fields.IsCardActive")]
        public bool IsCardActive { get; set; }
        public bool IsCardActive_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Unzer.Configuration.Fields.IsEpsActive")]
        public bool IsEpsActive { get; set; }
        public bool IsEpsActive_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Unzer.Configuration.Fields.IsPaypalActive")]
        public bool IsPaypalActive { get; set; }
        public bool IsPaypalActive_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Unzer.Configuration.Fields.PrivateKey")]
        public string ApiPrivateKey { get; set; }
        public bool ApiPrivateKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Unzer.Configuration.Fields.PublicKey")]
        public string ApiPublicKey { get; set; }
        public bool ApiPublicKey_OverrideForStore { get; set; }

        public int ActiveStoreScopeConfiguration { get; set; }
    }
}