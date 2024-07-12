using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.SMS.TeleSign.Areas.Admin.Models
{
    public record ConfigurationModel : BaseNopModel, ISettingsModel
    {
        [NopResourceDisplayName("Admin.NopStation.TeleSignSms.Configuration.Fields.EnablePlugin")]
        public bool EnablePlugin { get; set; }
        public bool EnablePlugin_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.TeleSignSms.Configuration.Fields.ApiKey")]
        public string ApiKey { get; set; }
        public bool ApiKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.TeleSignSms.Configuration.Fields.ApiSecret")]
        public string ApiSecret { get; set; }
        public bool ApiSecret_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.TeleSignSms.Configuration.Fields.PhoneNumber")]
        public string PhoneNumber { get; set; }
        public bool PhoneNumber_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.TeleSignSms.Configuration.Fields.From")]
        public string From { get; set; }
        public bool From_OverrideForStore { get; set; }
        
        [NopResourceDisplayName("Admin.NopStation.TeleSignSms.Configuration.Fields.CheckPhoneNumberRegex")]
        public bool CheckPhoneNumberRegex { get; set; }
        public bool CheckPhoneNumberRegex_OverrideForStore { get; set; }
        
        [NopResourceDisplayName("Admin.NopStation.TeleSignSms.Configuration.Fields.PhoneNumberRegex")]
        public string PhoneNumberRegex { get; set; }
        public bool PhoneNumberRegex_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.TeleSignSms.Configuration.Fields.CheckIntlDialCode")]
        public bool CheckIntlDialCode { get; set; }
        public bool CheckIntlDialCode_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.TeleSignSms.Configuration.Fields.IntlDialCode")]
        public string IntlDialCode { get; set; }
        public bool IntlDialCode_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.TeleSignSms.Configuration.Fields.RemoveFirstNDigitsWhenLocalNumber")]
        public int RemoveFirstNDigitsWhenLocalNumber { get; set; }
        public bool RemoveFirstNDigitsWhenLocalNumber_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.TeleSignSms.Configuration.Fields.EnableLog")]
        public bool EnableLog { get; set; }
        public bool EnableLog_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.TeleSignSms.Configuration.Fields.SendTestSmsTo")]
        public string SendTestSmsTo { get; set; }

        public int ActiveStoreScopeConfiguration { get; set; }
    }
}
