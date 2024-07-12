using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.SMS.Messente.Areas.Admin.Models
{
    public record ConfigurationModel : BaseNopModel, ISettingsModel
    {
        [NopResourceDisplayName("Admin.NopStation.Messente.Configuration.Fields.EnablePlugin")]
        public bool EnablePlugin { get; set; }
        public bool EnablePlugin_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Messente.Configuration.Fields.Username")]
        public string Username { get; set; }
        public bool Username_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Messente.Configuration.Fields.Password")]
        public string Password { get; set; }
        public bool Password_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Messente.Configuration.Fields.SenderName")]
        public string SenderName { get; set; }
        public bool SenderName_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Messente.Configuration.Fields.CheckPhoneNumberRegex")]
        public bool CheckPhoneNumberRegex { get; set; }
        public bool CheckPhoneNumberRegex_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Messente.Configuration.Fields.PhoneNumberRegex")]
        public string PhoneNumberRegex { get; set; }
        public bool PhoneNumberRegex_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Messente.Configuration.Fields.CheckIntlDialCode")]
        public bool CheckIntlDialCode { get; set; }
        public bool CheckIntlDialCode_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Messente.Configuration.Fields.IntlDialCode")]
        public string IntlDialCode { get; set; }
        public bool IntlDialCode_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Messente.Configuration.Fields.RemoveFirstNDigitsWhenLocalNumber")]
        public int RemoveFirstNDigitsWhenLocalNumber { get; set; }
        public bool RemoveFirstNDigitsWhenLocalNumber_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Messente.Configuration.Fields.EnableLog")]
        public bool EnableLog { get; set; }
        public bool EnableLog_OverrideForStore { get; set; }

        [NopResourceDisplayName("Admin.NopStation.Messente.Configuration.Fields.SendTestSmsTo")]
        public string SendTestSmsTo { get; set; }

        public int ActiveStoreScopeConfiguration { get; set; }
    }
}
