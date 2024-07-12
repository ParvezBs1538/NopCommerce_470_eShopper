using Nop.Core;

namespace NopStation.Plugin.AddressValidator.Google.Domains
{
    public class GoogleAddressExtension : BaseEntity
    {
        public int AddressId { get; set; }

        public string Longitude { get; set; }

        public string Latitude { get; set; }

        public string LocationType { get; set; }

        public string GooglePlaceId { get; set; }

        public string GoogleCompoundCode { get; set; }

        public string GoogleGlobalCode { get; set; }
    }
}
