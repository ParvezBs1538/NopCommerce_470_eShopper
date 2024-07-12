using System.Collections.Generic;
using System.Threading.Tasks;
using NopStation.Plugin.AddressValidator.Google.Domains;
using NopStation.Plugin.AddressValidator.Google.Services.Models;

namespace NopStation.Plugin.AddressValidator.Google.Services
{
    public interface IAddressExtensionService
    {
        Task DeleteAddressExtensionAsync(GoogleAddressExtension addressExtension);

        Task InsertAddressExtensionAsync(GoogleAddressExtension addressExtension);

        Task UpdateAddressExtension(GoogleAddressExtension addressExtension);

        Task<GoogleAddressExtension> GetAddressExtensionByIdAsync(int addressExtensionId);

        Task<GoogleAddressExtension> GetAddressExtensionByAddressIdAsync(int addressId);

        Task<IList<GoogleAddressExtension>> GetAddressExtensionsByAddressIdsAsync(int[] addressIds);

        Task<GeocodingResponse> GetGeocodingResponseAsync(string zipCode, string address1, string city, 
            int? stateProvinceId, int? countryId, string attributeXml);
    }
}