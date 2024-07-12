using Nop.Core.Configuration;

namespace NopStation.Plugin.AddressValidator.Google
{
    public class GoogleAddressValidatorSettings : ISettings
    {
        public bool EnablePlugin { get; set; }

        public string GoogleApiKey { get; set; }

        public bool ValidatePhoneNumber { get; set; }

        public string PhoneNumberRegex { get; set; }

        public int StreetNumberAttributeId { get; set; }

        public int StreetNameAttributeId { get; set; }

        public int PlotNumberAttributeId { get; set; }

        public bool EnableLog { get; set; }
    }
}
