using System.Collections.Generic;
using System.Threading.Tasks;
using NopStation.Plugin.AddressValidator.Byteplant.Domains;
using NopStation.Plugin.AddressValidator.Byteplant.Services.Models;

namespace NopStation.Plugin.AddressValidator.Byteplant.Services
{
    public interface IAddressExtensionService
    {
        Task DeleteAddressExtensionAsync(ByteplantAddressExtension addressExtension);

        Task InsertAddressExtensionAsync(ByteplantAddressExtension addressExtension);

        Task UpdateAddressExtension(ByteplantAddressExtension addressExtension);

        Task<ByteplantAddressExtension> GetAddressExtensionByIdAsync(int addressExtensionId);

        Task<ByteplantAddressExtension> GetAddressExtensionByAddressIdAsync(int addressId);

        Task<IList<ByteplantAddressExtension>> GetAddressExtensionsByAddressIdsAsync(int[] addressIds);

        Task<GeocodingResponse> GetGeocodingResponseAsync(string zipCode, string address1, string city, 
            int? stateProvinceId, int? countryId, string attributeXml);
    }
}