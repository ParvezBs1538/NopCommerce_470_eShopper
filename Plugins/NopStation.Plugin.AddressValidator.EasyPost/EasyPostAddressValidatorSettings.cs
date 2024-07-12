using Nop.Core.Configuration;

namespace NopStation.Plugin.AddressValidator.EasyPost
{
    public class EasyPostAddressValidatorSettings : ISettings
    {
        public bool EnablePlugin { get; set; }

        public string EasyPostApiKey { get; set; }

        public bool EnableLog { get; set; }
    }
}
