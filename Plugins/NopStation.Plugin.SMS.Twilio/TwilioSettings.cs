using Nop.Core.Configuration;

namespace NopStation.Plugin.SMS.Twilio
{
    public class TwilioSettings : ISettings
    {
        public bool EnablePlugin { get; set; }

        public string SubAccount { get; set; }

        public string AuthToken { get; set; }

        public string AccountSID { get; set; }

        public string PhoneNumber { get; set; }

        public bool CheckPhoneNumberRegex { get; set; }

        public string PhoneNumberRegex { get; set; }

        public bool CheckIntlDialCode { get; set; }

        public string IntlDialCode { get; set; }

        public int RemoveFirstNDigitsWhenLocalNumber { get; set; }

        public bool EnableLog { get; set; }
    }
}
