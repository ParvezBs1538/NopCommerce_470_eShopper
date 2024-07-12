﻿using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.SMS.Twilio.Areas.Admin.Models
{
    public record ConfigurationModel : BaseNopModel, ISettingsModel
    {
        [NopResourceDisplayName("Admin.NopStation.Twilio.Configuration.Fields.EnablePlugin")]
        public bool EnablePlugin { get; set; }
        public bool EnablePlugin_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Twilio.Configuration.Fields.SubAccount")]
        public string SubAccount { get; set; }
        public bool SubAccount_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Twilio.Configuration.Fields.AuthToken")]
        public string AuthToken { get; set; }
        public bool AuthToken_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Twilio.Configuration.Fields.AccountSID")]
        public string AccountSID { get; set; }
        public bool AccountSID_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Twilio.Configuration.Fields.PhoneNumber")]
        public string PhoneNumber { get; set; }
        public bool PhoneNumber_OverrideForStore { get; set; }
        
        [NopResourceDisplayName("Admin.NopStation.Twilio.Configuration.Fields.CheckPhoneNumberRegex")]
        public bool CheckPhoneNumberRegex { get; set; }
        public bool CheckPhoneNumberRegex_OverrideForStore { get; set; }
        
        [NopResourceDisplayName("Admin.NopStation.Twilio.Configuration.Fields.PhoneNumberRegex")]
        public string PhoneNumberRegex { get; set; }
        public bool PhoneNumberRegex_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Twilio.Configuration.Fields.CheckIntlDialCode")]
        public bool CheckIntlDialCode { get; set; }
        public bool CheckIntlDialCode_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Twilio.Configuration.Fields.IntlDialCode")]
        public string IntlDialCode { get; set; }
        public bool IntlDialCode_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Twilio.Configuration.Fields.RemoveFirstNDigitsWhenLocalNumber")]
        public int RemoveFirstNDigitsWhenLocalNumber { get; set; }
        public bool RemoveFirstNDigitsWhenLocalNumber_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Twilio.Configuration.Fields.EnableLog")]
        public bool EnableLog { get; set; }
        public bool EnableLog_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Twilio.Configuration.Fields.SendTestSmsTo")]
        public string SendTestSmsTo { get; set; }

        public int ActiveStoreScopeConfiguration { get; set; }
    }
}
