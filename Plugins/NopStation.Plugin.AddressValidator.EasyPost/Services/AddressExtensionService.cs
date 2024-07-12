using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nop.Data;
using Nop.Services.Directory;
using Nop.Services.Logging;
using NopStation.Plugin.AddressValidator.EasyPost.Domains;
using NopStation.Plugin.AddressValidator.EasyPost.Services.Models;

namespace NopStation.Plugin.AddressValidator.EasyPost.Services
{
    public class AddressExtensionService : IAddressExtensionService
    {
        #region Fields

        private readonly IRepository<EasyPostAddressExtension> _addressExtensionRepository;
        private readonly ICountryService _countryService;
        private readonly ILogger _logger;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly EasyPostAddressValidatorSettings _addressValidatorSettings;

        #endregion

        #region Ctor

        public AddressExtensionService(EasyPostAddressValidatorSettings addressValidatorSettings,
            IRepository<EasyPostAddressExtension> addressExtensionRepository,
            ICountryService countryService,
            ILogger logger,
            IStateProvinceService stateProvinceService)
        {
            _addressExtensionRepository = addressExtensionRepository;
            _countryService = countryService;
            _logger = logger;
            _stateProvinceService = stateProvinceService;
            _addressValidatorSettings = addressValidatorSettings;
        }

        #endregion

        #region Methods

        public async Task DeleteAddressExtensionAsync(EasyPostAddressExtension addressExtension)
        {
            await _addressExtensionRepository.DeleteAsync(addressExtension);
        }

        public async Task InsertAddressExtensionAsync(EasyPostAddressExtension addressExtension)
        {
            await _addressExtensionRepository.InsertAsync(addressExtension);
        }

        public async Task UpdateAddressExtension(EasyPostAddressExtension addressExtension)
        {
            await _addressExtensionRepository.UpdateAsync(addressExtension);
        }

        public async Task<EasyPostAddressExtension> GetAddressExtensionByIdAsync(int addressExtensionId)
        {
            if (addressExtensionId == 0)
                return null;

            return await _addressExtensionRepository.GetByIdAsync(addressExtensionId);
        }

        public async Task<EasyPostAddressExtension> GetAddressExtensionByAddressIdAsync(int addressId)
        {
            if (addressId == 0)
                return null;

            return await _addressExtensionRepository.Table.Where(x => x.AddressId == addressId).FirstOrDefaultAsync();
        }

        public async Task<IList<EasyPostAddressExtension>> GetAddressExtensionsByAddressIdsAsync(int[] addressIds)
        {
            return await _addressExtensionRepository.Table.Where(x => addressIds.Contains(x.AddressId)).ToListAsync();
        }

        public async Task<GeocodingResponse> GetGeocodingResponseAsync(string zipCode, string address1, string address2, 
            string phone, string city, int? stateProvinceId, int? countryId, string company, string attributeXml)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(new HttpMethod("POST"), "https://api.easypost.com/v2/addresses"))
                    {
                        var base64authorization = Convert.ToBase64String(Encoding.ASCII.GetBytes(_addressValidatorSettings.EasyPostApiKey));
                        request.Headers.TryAddWithoutValidation("Authorization", $"Basic {base64authorization}");

                        var contentList = new List<string>
                        {
                            "verify[]=delivery",
                            $"address[street1]={address1}",
                            $"address[street2]={address2}",
                            $"address[city]={city}",
                            $"address[state]={(await _stateProvinceService.GetStateProvinceByIdAsync(stateProvinceId ?? 0))?.Abbreviation}",
                            $"address[zip]={zipCode}",
                            $"address[country]={(await _countryService.GetCountryByIdAsync(countryId ?? 0))?.TwoLetterIsoCode}",
                            $"address[company]={company}",
                            $"address[phone]={phone}"
                        };
                        request.Content = new StringContent(string.Join("&", contentList));
                        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");

                        var response = await httpClient.SendAsync(request);
                        var body = await response.Content.ReadAsStringAsync();

                        if (_addressValidatorSettings.EnableLog)
                            await _logger.InformationAsync("Validation response: " + body);

                        return JsonConvert.DeserializeObject<GeocodingResponse>(body);
                    }
                }
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync(ex.Message, ex);
                return new GeocodingResponse();
            }
        }

        #endregion
    }
}
