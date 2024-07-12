using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.EmailValidator.Verifalia.Models
{
    public record ConfigurationModel : BaseNopModel, ISettingsModel
    {
        public ConfigurationModel()
        {
            AvailableQualityLevels = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.NopStation.VerifaliaEmailValidator.Configuration.Fields.EnablePlugin")]
        public bool EnablePlugin { get; set; }
        public bool EnablePlugin_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VerifaliaEmailValidator.Configuration.Fields.ValidateCustomerInfoEmail")]
        public bool ValidateCustomerInfoEmail { get; set; }
        public bool ValidateCustomerInfoEmail_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VerifaliaEmailValidator.Configuration.Fields.ValidateCustomerAddressEmail")]
        public bool ValidateCustomerAddressEmail { get; set; }
        public bool ValidateCustomerAddressEmail_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VerifaliaEmailValidator.Configuration.Fields.Username")]
        public string Username { get; set; }
        public bool Username_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VerifaliaEmailValidator.Configuration.Fields.Password")]
        public string Password { get; set; }
        public bool Password_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VerifaliaEmailValidator.Configuration.Fields.ValidateQuality")]
        public bool ValidateQuality { get; set; }
        public bool ValidateQuality_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VerifaliaEmailValidator.Configuration.Fields.QualityLevel")]
        public string QualityLevel { get; set; }
        public bool QualityLevel_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VerifaliaEmailValidator.Configuration.Fields.EnableLog")]
        public bool EnableLog { get; set; }
        public bool EnableLog_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VerifaliaEmailValidator.Configuration.Fields.AllowRiskyEmails")]
        public bool AllowRiskyEmails { get; set; }
        public bool AllowRiskyEmails_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VerifaliaEmailValidator.Configuration.Fields.BlockedDomains")]
        public string BlockedDomains { get; set; }
        public bool BlockedDomains_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.VerifaliaEmailValidator.Configuration.Fields.RevalidateInvalidEmailsAfterHours")]
        public int RevalidateInvalidEmailsAfterHours { get; set; }
        public bool RevalidateInvalidEmailsAfterHours_OverrideForStore { get; set; }

        public IList<SelectListItem> AvailableQualityLevels { get; set; }

        public int ActiveStoreScopeConfiguration { get; set; }
    }
}
