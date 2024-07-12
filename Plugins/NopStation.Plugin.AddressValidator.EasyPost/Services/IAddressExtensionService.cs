using System.Collections.Generic;
using System.Threading.Tasks;
using NopStation.Plugin.AddressValidator.EasyPost.Domains;
using NopStation.Plugin.AddressValidator.EasyPost.Services.Models;

namespace NopStation.Plugin.AddressValidator.EasyPost.Services
{
    public interface IAddressExtensionService
    {
        Task DeleteAddressExtensionAsync(EasyPostAddressExtension addressExtension);

        Task InsertAddressExtensionAsync(EasyPostAddressExtension addressExtension);

        Task UpdateAddressExtension(EasyPostAddressExtension addressExtension);

        Task<EasyPostAddressExtension> GetAddressExtensionByIdAsync(int addressExtensionId);

        Task<EasyPostAddressExtension> GetAddressExtensionByAddressIdAsync(int addressId);

        Task<IList<EasyPostAddressExtension>> GetAddressExtensionsByAddressIdsAsync(int[] addressIds);

        Task<GeocodingResponse> GetGeocodingResponseAsync(string zipCode, string address1, string address2,
            string phone, string city, int? stateProvinceId, int? countryId, string company, string attributeXml);
    }
}