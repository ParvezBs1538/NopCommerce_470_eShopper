using System.Collections.Generic;
using Nop.Core.Configuration;

namespace NopStation.Plugin.EmailValidator.Verifalia
{
    public class VerifaliaEmailValidatorSettings : ISettings
    {
        public VerifaliaEmailValidatorSettings()
        {
            BlockedDomains = new List<string>();
        }

        public bool EnablePlugin { get; set; }

        public bool ValidateCustomerInfoEmail { get; set; }

        public bool ValidateCustomerAddressEmail { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public bool ValidateQuality { get; set; }

        public string QualityLevel { get; set; }

        public bool AllowRiskyEmails { get; set; }

        public List<string> BlockedDomains { get; set; }

        public int RevalidateInvalidEmailsAfterHours { get; set; }

        public bool EnableLog { get; set; }
    }
}
