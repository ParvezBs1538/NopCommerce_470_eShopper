using Nop.Core.Configuration;

namespace NopStation.Plugin.AddressValidator.Byteplant
{
    public class ByteplantAddressValidatorSettings : ISettings
    {
        public bool EnablePlugin { get; set; }

        public string ByteplantApiKey { get; set; }

        public bool ValidatePhoneNumber { get; set; }

        public string PhoneNumberRegex { get; set; }

        public bool EnableLog { get; set; }
    }
}
