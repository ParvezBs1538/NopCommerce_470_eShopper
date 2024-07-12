using System.Collections.Generic;
using Nop.Core.Configuration;

namespace NopStation.Plugin.EmailValidator.Abstract
{
    public class AbstractEmailValidatorSettings : ISettings
    {
        public AbstractEmailValidatorSettings()
        {
            BlockedDomains = new List<string>();
        }

        public bool EnablePlugin { get; set; }

        public bool ValidateCustomerInfoEmail { get; set; }

        public bool ValidateCustomerAddressEmail { get; set; }

        public string ApiKey { get; set; }

        public bool AllowRiskyEmails { get; set; }

        public List<string> BlockedDomains { get; set; }

        public int RevalidateInvalidEmailsAfterHours { get; set; }

        public bool EnableLog { get; set; }
    }
}
