using Nop.Core.Configuration;

namespace NopStation.Plugin.SMS.Vonage
{
    public class VonageSettings : ISettings
    {
        public bool EnablePlugin { get; set; }

        public string ApiKey { get; set; }

        public string ApiSecret { get; set; }

        /// <summary>
        /// The name or number the message should be sent from. 
        /// Alphanumeric senderID's are not supported in all countries, see Global Messaging for more details. 
        /// If alphanumeric, spaces will be ignored. Numbers are specified in E.164 format.
        /// </summary>
        public string From { get; set; }

        public bool CheckPhoneNumberRegex { get; set; }

        public string PhoneNumberRegex { get; set; }

        public bool CheckIntlDialCode { get; set; }

        public string IntlDialCode { get; set; }

        public int RemoveFirstNDigitsWhenLocalNumber { get; set; }

        public bool EnableLog { get; set; }
    }
}
