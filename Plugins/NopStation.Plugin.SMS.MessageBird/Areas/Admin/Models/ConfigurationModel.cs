using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.SMS.MessageBird.Areas.Admin.Models
{
    public record ConfigurationModel : BaseNopModel, ISettingsModel
    {
        [NopResourceDisplayName("Admin.NopStation.MessageBird.Configuration.Fields.EnablePlugin")]
        public bool EnablePlugin { get; set; }
        public bool EnablePlugin_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.MessageBird.Configuration.Fields.Originator")]
        public string Originator { get; set; }
        public bool Originator_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.MessageBird.Configuration.Fields.AccessKey")]
        public string AccessKey { get; set; }
        public bool AccessKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.MessageBird.Configuration.Fields.CheckPhoneNumberRegex")]
        public bool CheckPhoneNumberRegex { get; set; }
        public bool CheckPhoneNumberRegex_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.MessageBird.Configuration.Fields.PhoneNumberRegex")]
        public string PhoneNumberRegex { get; set; }
        public bool PhoneNumberRegex_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.MessageBird.Configuration.Fields.CheckIntlDialCode")]
        public bool CheckIntlDialCode { get; set; }
        public bool CheckIntlDialCode_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.MessageBird.Configuration.Fields.IntlDialCode")]
        public string IntlDialCode { get; set; }
        public bool IntlDialCode_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.MessageBird.Configuration.Fields.RemoveFirstNDigitsWhenLocalNumber")]
        public int RemoveFirstNDigitsWhenLocalNumber { get; set; }
        public bool RemoveFirstNDigitsWhenLocalNumber_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.MessageBird.Configuration.Fields.EnableLog")]
        public bool EnableLog { get; set; }
        public bool EnableLog_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.MessageBird.Configuration.Fields.SendTestSmsTo")]
        public string SendTestSmsTo { get; set; }

        public int ActiveStoreScopeConfiguration { get; set; }
    }
}
