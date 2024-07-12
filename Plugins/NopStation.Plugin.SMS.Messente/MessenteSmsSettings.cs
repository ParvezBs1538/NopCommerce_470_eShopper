using Nop.Core.Configuration;

namespace NopStation.Plugin.SMS.Messente
{
    public class MessenteSmsSettings : ISettings
    {
        public bool EnablePlugin { get; set; }

        public string Password { get; set; }

        public string Username { get; set; }

        public string SenderName { get; set; }

        public bool CheckPhoneNumberRegex { get; set; }

        public string PhoneNumberRegex { get; set; }

        public bool CheckIntlDialCode { get; set; }

        public string IntlDialCode { get; set; }

        public int RemoveFirstNDigitsWhenLocalNumber { get; set; }

        public bool EnableLog { get; set; }
    }
}
