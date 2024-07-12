using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.EmailValidator.Abstract.Models
{
    public record ConfigurationModel : BaseNopModel, ISettingsModel
    {
        [NopResourceDisplayName("Admin.NopStation.AbstractEmailValidator.Configuration.Fields.EnablePlugin")]
        public bool EnablePlugin { get; set; }
        public bool EnablePlugin_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AbstractEmailValidator.Configuration.Fields.ValidateCustomerInfoEmail")]
        public bool ValidateCustomerInfoEmail { get; set; }
        public bool ValidateCustomerInfoEmail_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AbstractEmailValidator.Configuration.Fields.ValidateCustomerAddressEmail")]
        public bool ValidateCustomerAddressEmail { get; set; }
        public bool ValidateCustomerAddressEmail_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AbstractEmailValidator.Configuration.Fields.ApiKey")]
        public string ApiKey { get; set; }
        public bool ApiKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AbstractEmailValidator.Configuration.Fields.EnableLog")]
        public bool EnableLog { get; set; }
        public bool EnableLog_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AbstractEmailValidator.Configuration.Fields.AllowRiskyEmails")]
        public bool AllowRiskyEmails { get; set; }
        public bool AllowRiskyEmails_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AbstractEmailValidator.Configuration.Fields.BlockedDomains")]
        public string BlockedDomains { get; set; }
        public bool BlockedDomains_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.AbstractEmailValidator.Configuration.Fields.RevalidateInvalidEmailsAfterHours")]
        public int RevalidateInvalidEmailsAfterHours { get; set; }
        public bool RevalidateInvalidEmailsAfterHours_OverrideForStore { get; set; }

        public int ActiveStoreScopeConfiguration { get; set; }
    }
}
