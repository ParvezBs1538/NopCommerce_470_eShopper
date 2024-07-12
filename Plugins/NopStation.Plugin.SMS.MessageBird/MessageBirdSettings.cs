using Nop.Core.Configuration;

namespace NopStation.Plugin.SMS.MessageBird
{
    public class MessageBirdSettings : ISettings
    {
        public bool EnablePlugin { get; set; }

        public string AccessKey { get; set; }

        public string Originator { get; set; }

        public bool CheckPhoneNumberRegex { get; set; }

        public string PhoneNumberRegex { get; set; }

        public bool CheckIntlDialCode { get; set; }

        public string IntlDialCode { get; set; }

        public int RemoveFirstNDigitsWhenLocalNumber { get; set; }

        public bool EnableLog { get; set; }
    }
}
