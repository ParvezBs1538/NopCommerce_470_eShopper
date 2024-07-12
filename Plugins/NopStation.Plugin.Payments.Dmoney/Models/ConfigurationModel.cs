using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.Payments.Dmoney.Models
{
    public record ConfigurationModel : BaseNopModel, ISettingsModel
    {
        [NopResourceDisplayName("Admin.NopStation.DmoneyPayment.Configuration.Fields.GatewayUrl")]
        public string GatewayUrl { get; set; }
        public bool GatewayUrl_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DmoneyPayment.Configuration.Fields.TransactionVerificationUrl")]
        public string TransactionVerificationUrl { get; set; }
        public bool TransactionVerificationUrl_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DmoneyPayment.Configuration.Fields.OrganizationCode")]
        public string OrganizationCode { get; set; }
        public bool OrganizationCode_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DmoneyPayment.Configuration.Fields.Password")]
        public string Password { get; set; }
        public bool Password_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DmoneyPayment.Configuration.Fields.SecretKey")]
        public string SecretKey { get; set; }
        public bool SecretKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DmoneyPayment.Configuration.Fields.BillerCode")]
        public string BillerCode { get; set; }
        public bool BillerCode_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.DmoneyPayment.Configuration.Fields.Description")]
        public string Description { get; set; }
        public bool Description_OverrideForStore { get; set; }

        public int ActiveStoreScopeConfiguration { get; set; }
    }
}
