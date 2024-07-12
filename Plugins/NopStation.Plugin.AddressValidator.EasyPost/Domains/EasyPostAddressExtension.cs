using Nop.Core;

namespace NopStation.Plugin.AddressValidator.EasyPost.Domains
{
    public class EasyPostAddressExtension : BaseEntity
    {
        public int AddressId { get; set; }

        public string Longitude { get; set; }

        public string Latitude { get; set; }

        public string TimeZone { get; set; }
    }
}
