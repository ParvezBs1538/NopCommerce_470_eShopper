using Nop.Data;
using NopStation.Plugin.AddressValidator.Google.Domains;
using System;
using System.Linq.Dynamic.Core;
using System.Linq;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using NopStation.Plugin.AddressValidator.Google.Services.Models;
using Nop.Services.Directory;
using Nop.Services.Logging;
using System.Collections.Generic;
using Nop.Services.Common;
using System.Threading.Tasks;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Catalog;
using Nop.Services.Localization;
using Nop.Core;
using Nop.Services.Attributes;

namespace NopStation.Plugin.AddressValidator.Google.Services
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

        private readonly IRepository<GoogleAddressExtension> _addressExtensionRepository;
        private readonly ICountryService _countryService;
        private readonly ILogger _logger;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IAttributeService<AddressAttribute, AddressAttributeValue> _addressAttributeService;
        private readonly IAttributeParser<AddressAttribute, AddressAttributeValue> _addressAttributeParser;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly GoogleAddressValidatorSettings _addressValidatorSettings;

        #endregion

        #region Ctor

        public AddressExtensionService(GoogleAddressValidatorSettings addressValidatorSettings,
            IRepository<GoogleAddressExtension> addressExtensionRepository,
            ICountryService countryService,
            ILogger logger,
            IStateProvinceService stateProvinceService,
            IAttributeService<AddressAttribute, AddressAttributeValue> addressAttributeService,
            IAttributeParser<AddressAttribute, AddressAttributeValue> addressAttributeParser,
            ILocalizationService localizationService,
            IWorkContext workContext)
        {
            _addressExtensionRepository = addressExtensionRepository;
            _countryService = countryService;
            _logger = logger;
            _stateProvinceService = stateProvinceService;
            _addressAttributeService = addressAttributeService;
            _addressAttributeParser = addressAttributeParser;
            _localizationService = localizationService;
            _workContext = workContext;
            _addressValidatorSettings = addressValidatorSettings;
        }

        #endregion

        #region Utilities

        protected async Task<(string Address, string Component)> GetFullAddressAsync(string zipCode, string address1, string city,
            int? stateProvinceId, int? countryId, string attributesXml)
        {
            var addressItems = new List<AddressItem>();

            if (!string.IsNullOrWhiteSpace(address1))
                addressItems.Add(new AddressItem() { Name = address1, Order = 4 });

            var requiredAttributes = await _addressAttributeService.GetAllAttributesAsync();
            var attributes = await _addressAttributeParser.ParseAttributesAsync(attributesXml);

            foreach (var requiredAttribute in requiredAttributes)
            {
                if (requiredAttribute.Id == _addressValidatorSettings.PlotNumberAttributeId && _addressValidatorSettings.PlotNumberAttributeId > 0)
                {
                    var values = await GetAttributeValuesAsync(attributesXml, requiredAttribute, attributes);
                    if (!string.IsNullOrWhiteSpace(values))
                    {
                        addressItems.Add(new AddressItem()
                        {
                            Name = values,
                            Order = 1,
                            Key = "subpremise"
                        });
                    }
                    continue;
                }
                if (requiredAttribute.Id == _addressValidatorSettings.StreetNameAttributeId && _addressValidatorSettings.StreetNameAttributeId > 0)
                {
                    var values = await GetAttributeValuesAsync(attributesXml, requiredAttribute, attributes);
                    if (!string.IsNullOrWhiteSpace(values))
                    {
                        addressItems.Add(new AddressItem()
                        {
                            Name = values,
                            Order = 2,
                            Key = "route"
                        });
                    }
                    continue;
                }
                if (requiredAttribute.Id == _addressValidatorSettings.StreetNumberAttributeId && _addressValidatorSettings.StreetNumberAttributeId > 0)
                {
                    var values = await GetAttributeValuesAsync(attributesXml, requiredAttribute, attributes);
                    if (!string.IsNullOrWhiteSpace(values))
                    {
                        addressItems.Add(new AddressItem()
                        {
                            Name = values,
                            Order = 3,
                            Key = "street_number"
                        });
                    }
                    continue;
                }
            }

            if (!string.IsNullOrWhiteSpace(zipCode))
                addressItems.Add(new AddressItem()
                {
                    Name = zipCode,
                    Order = 0,
                    Key = "postal_code"
                });

            if (!string.IsNullOrWhiteSpace(city))
                addressItems.Add(new AddressItem()
                {
                    Name = WebUtility.UrlEncode(city),
                    Order = 5,
                    Key = "administrative_area_level_2"
                });

            if (stateProvinceId.HasValue)
            {
                var stateProvince = await _stateProvinceService.GetStateProvinceByIdAsync(stateProvinceId.Value);
                if (stateProvince != null && stateProvince.Published)
                {
                    addressItems.Add(new AddressItem()
                    {
                        Name = WebUtility.UrlEncode(stateProvince.Name),
                        Order = 6,
                        Key = "administrative_area_level_1"
                    });
                }
            }

            if (countryId.HasValue)
            {
                var country = await _countryService.GetCountryByIdAsync(countryId.Value);
                if (country != null && country.Published)
                {
                    addressItems.Add(new AddressItem()
                    {
                        Name = WebUtility.UrlEncode(country.Name),
                        Order = 7,
                        Key = "country"
                    });
                }
            }

            var orderedAddressItems = addressItems.OrderBy(x => x.Order).ToList();

            var component = "";
            if (orderedAddressItems.Any())
            {
                component = "components=" + string.Join("|", orderedAddressItems
                    .Where(x => !string.IsNullOrWhiteSpace(x.Key))
                    .Select(x => x.Key + ":" + x.Name)) + "&";
            }

            return (WebUtility.UrlEncode(string.Join(", ", orderedAddressItems.Select(x => x.Name))), component);
        }

        private async Task<string> GetAttributeValuesAsync(string attributesXml, AddressAttribute requiredAttribute, IList<AddressAttribute> attributes)
        {
            var attribute = attributes.FirstOrDefault(x => x.Id == requiredAttribute.Id);
            if (attribute == null)
                return null;

            var valuesStr = _addressAttributeParser.ParseValues(attributesXml, attribute.Id);
            for (var j = 0; j < valuesStr.Count; j++)
            {
                var valueStr = valuesStr[j];
                var formattedAttribute = string.Empty;
                if (!attribute.ShouldHaveValues)
                {
                    if (attribute.AttributeControlType != AttributeControlType.FileUpload)
                    return valueStr;
                }
                else
                {
                    if (int.TryParse(valueStr, out var attributeValueId))
                    {
                        var attributeValue = await _addressAttributeService.GetAttributeValueByIdAsync(attributeValueId);
                        if (attributeValue != null)
                            return await _localizationService.GetLocalizedAsync(attributeValue, a => a.Name, (await _workContext.GetWorkingLanguageAsync()).Id);
                    }
                }
            }

            return "";
        }

        #endregion

        #region Methods

        public async Task DeleteAddressExtensionAsync(GoogleAddressExtension addressExtension)
        {
            await _addressExtensionRepository.DeleteAsync(addressExtension);
        }

        public async Task InsertAddressExtensionAsync(GoogleAddressExtension addressExtension)
        {
            await _addressExtensionRepository.InsertAsync(addressExtension);
        }

        public async Task UpdateAddressExtension(GoogleAddressExtension addressExtension)
        {
            await _addressExtensionRepository.UpdateAsync(addressExtension);
        }

        public async Task<GoogleAddressExtension> GetAddressExtensionByIdAsync(int addressExtensionId)
        {
            if (addressExtensionId == 0)
                return null;

            return await _addressExtensionRepository.GetByIdAsync(addressExtensionId);
        }

        public async Task<GoogleAddressExtension> GetAddressExtensionByAddressIdAsync(int addressId)
        {
            if (addressId == 0)
                return null;

            return await _addressExtensionRepository.Table.Where(x => x.AddressId == addressId).FirstOrDefaultAsync();
        }

        public async Task<IList<GoogleAddressExtension>> GetAddressExtensionsByAddressIdsAsync(int[] addressIds)
        {
            return await _addressExtensionRepository.Table.Where(x => addressIds.Contains(x.AddressId)).ToListAsync();
        }

        public async Task<GeocodingResponse> GetGeocodingResponseAsync(string zipCode, string address1, string city,
            int? stateProvinceId, int? countryId, string attributeXml)
        {
            var fullAddress = await GetFullAddressAsync(zipCode, address1, city, stateProvinceId, countryId, attributeXml);
            var url = $"https://maps.googleapis.com/maps/api/geocode/json?{fullAddress.Component}address={fullAddress.Address}&key={_addressValidatorSettings.GoogleApiKey}";
            var request = WebRequest.Create(url);

            using (var response = request.GetResponse())
            {
                using (var dataStream = response.GetResponseStream())
                {
                    var reader = new StreamReader(dataStream);
                    var rt = reader.ReadToEnd();
                    reader.Close();
                    response.Close();

                    if (_addressValidatorSettings.EnableLog)
                        await _logger.InformationAsync($"Url: {url} {Environment.NewLine + Environment.NewLine}Address: {fullAddress} " +
                            $"{Environment.NewLine + Environment.NewLine}Response: {rt}");

                    var responseModel = JsonConvert.DeserializeObject<GeocodingResponse>(rt);
                    return responseModel;
                }
            }
        }

        #endregion
    }
}
