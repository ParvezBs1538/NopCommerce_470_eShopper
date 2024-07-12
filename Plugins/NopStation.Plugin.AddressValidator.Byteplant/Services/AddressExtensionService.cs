using Nop.Data;
using NopStation.Plugin.AddressValidator.Byteplant.Domains;
using System;
using System.Linq.Dynamic.Core;
using System.Linq;
using Newtonsoft.Json;
using NopStation.Plugin.AddressValidator.Byteplant.Services.Models;
using Nop.Services.Directory;
using Nop.Services.Logging;
using System.Collections.Generic;
using Nop.Services.Common;
using System.Threading.Tasks;
using Nop.Services.Localization;
using Nop.Core;
using System.Net.Http;
using Nop.Core.Domain.Directory;
using Nop.Services.Attributes;
using Nop.Core.Domain.Common;

namespace NopStation.Plugin.AddressValidator.Byteplant.Services
{
    public class AddressItem
    {
        public int Order { get; set; }

        public string Key { get; set; }

        public string Name { get; set; }
    }

    public class AddressExtensionService : IAddressExtensionService
    {
        #region Fields

        private readonly IRepository<ByteplantAddressExtension> _addressExtensionRepository;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IWorkContext _workContext;
        private readonly ByteplantAddressValidatorSettings _addressValidatorSettings;

        #endregion

        #region Ctor

        public AddressExtensionService(ByteplantAddressValidatorSettings addressValidatorSettings,
            IRepository<ByteplantAddressExtension> addressExtensionRepository,
            ICountryService countryService,
            IStateProvinceService stateProvinceService,
            IWorkContext workContext)
        {
            _addressExtensionRepository = addressExtensionRepository;
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
            _workContext = workContext;
            _addressValidatorSettings = addressValidatorSettings;
        }

        #endregion

        #region Methods

        public async Task DeleteAddressExtensionAsync(ByteplantAddressExtension addressExtension)
        {
            await _addressExtensionRepository.DeleteAsync(addressExtension);
        }

        public async Task InsertAddressExtensionAsync(ByteplantAddressExtension addressExtension)
        {
            await _addressExtensionRepository.InsertAsync(addressExtension);
        }

        public async Task UpdateAddressExtension(ByteplantAddressExtension addressExtension)
        {
            await _addressExtensionRepository.UpdateAsync(addressExtension);
        }

        public async Task<ByteplantAddressExtension> GetAddressExtensionByIdAsync(int addressExtensionId)
        {
            if (addressExtensionId == 0)
                return null;

            return await _addressExtensionRepository.GetByIdAsync(addressExtensionId);
        }

        public async Task<ByteplantAddressExtension> GetAddressExtensionByAddressIdAsync(int addressId)
        {
            if (addressId == 0)
                return null;

            return await _addressExtensionRepository.Table.Where(x => x.AddressId == addressId).FirstOrDefaultAsync();
        }

        public async Task<IList<ByteplantAddressExtension>> GetAddressExtensionsByAddressIdsAsync(int[] addressIds)
        {
            return await _addressExtensionRepository.Table.Where(x => addressIds.Contains(x.AddressId)).ToListAsync();
        }

        public async Task<GeocodingResponse> GetGeocodingResponseAsync(string zipCode, string address1, string city,
            int? stateProvinceId, int? countryId, string attributeXml)
        {
            var state = "";
            var countryCode = "";

            if (countryId.HasValue && await _countryService.GetCountryByIdAsync(countryId.Value) is Country country)
                countryCode = country.TwoLetterIsoCode;
            if (stateProvinceId.HasValue && await _stateProvinceService.GetStateProvinceByIdAsync(stateProvinceId.Value) is StateProvince stateProvince)
                countryCode = stateProvince.Name;

            var client = new HttpClient();

            var postData = new List<KeyValuePair<string, string>>();
            postData.Add(new KeyValuePair<string, string>("StreetAddress", address1));
            postData.Add(new KeyValuePair<string, string>("City", city));
            postData.Add(new KeyValuePair<string, string>("PostalCode", zipCode));
            postData.Add(new KeyValuePair<string, string>("State", state));
            postData.Add(new KeyValuePair<string, string>("CountryCode", countryCode));
            postData.Add(new KeyValuePair<string, string>("Locale", (await _workContext.GetWorkingLanguageAsync()).LanguageCulture));
            postData.Add(new KeyValuePair<string, string>("APIKey", _addressValidatorSettings.ByteplantApiKey));

            var content = new FormUrlEncodedContent(postData);

            var result = await client.PostAsync("https://api.address-validator.net/api/verify", content);
            var resultContent = await result.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<GeocodingResponse>(resultContent);
        }

        #endregion
    }
}
