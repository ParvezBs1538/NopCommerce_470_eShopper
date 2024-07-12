using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using NopStation.Plugin.Widgets.AffiliateStation.Domains;
using NopStation.Plugin.Widgets.AffiliateStation.Models;
using NopStation.Plugin.Widgets.AffiliateStation.Services;
using Nop.Services.Affiliates;
using Nop.Services.Common;
using Nop.Services.Directory;
using Nop.Services.Localization;

namespace NopStation.Plugin.Widgets.AffiliateStation.Factories
{
    public class AffiliateCustomerModelFactory : IAffiliateCustomerModelFactory
    {
        #region Fields

        private readonly IWorkContext _workContext;
        private readonly IAffiliateCustomerService _affiliateCustomerService;
        private readonly IAffiliateService _affiliateService;
        private readonly ILocalizationService _localizationService;
        private readonly AddressSettings _addressSettings;
        private readonly ICountryService _countryService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IAddressService _addressService;
        private readonly CustomerSettings _customerSettings;

        #endregion

        #region Ctor

        public AffiliateCustomerModelFactory(IWorkContext workContext,
            IAffiliateCustomerService affiliateCustomerService,
            IAffiliateService affiliateService,
            ILocalizationService localizationService,
            AddressSettings addressSettings,
            ICountryService countryService,
            IGenericAttributeService genericAttributeService,
            IStateProvinceService stateProvinceService,
            IAddressService addressService,
            CustomerSettings customerSettings)
        {
            _workContext = workContext;
            _affiliateCustomerService = affiliateCustomerService;
            _affiliateService = affiliateService;
            _localizationService = localizationService;
            _addressSettings = addressSettings;
            _countryService = countryService;
            _genericAttributeService = genericAttributeService;
            _stateProvinceService = stateProvinceService;
            _addressService = addressService;
            _customerSettings = customerSettings;
        }

        #endregion

        #region Utilities

        protected virtual async Task PrepareAddressModelAsync(AffiliateInfoModel.AffiliateAddressModel model,
            Address address, bool excludeProperties,
            Func<Task<IList<Country>>> loadCountriesAsync = null,
            bool prePopulateWithCustomerFields = false,
            Customer customer = null,
            string overrideAttributesXml = "")
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (!excludeProperties && address != null)
            {
                model.Id = address.Id;
                model.FirstName = address.FirstName;
                model.LastName = address.LastName;
                model.Email = address.Email;
                model.Company = address.Company;
                model.CountryId = address.CountryId;
                model.CountryName = await _countryService.GetCountryByAddressAsync(address) is Country country ? await _localizationService.GetLocalizedAsync(country, x => x.Name) : null;
                model.StateProvinceId = address.StateProvinceId;
                model.StateProvinceName = await _stateProvinceService.GetStateProvinceByAddressAsync(address) is StateProvince stateProvince ? await _localizationService.GetLocalizedAsync(stateProvince, x => x.Name) : null;
                model.County = address.County;
                model.City = address.City;
                model.Address1 = address.Address1;
                model.Address2 = address.Address2;
                model.ZipPostalCode = address.ZipPostalCode;
                model.PhoneNumber = address.PhoneNumber;
                model.FaxNumber = address.FaxNumber;
            }

            if (address == null && prePopulateWithCustomerFields)
            {
                if (customer == null)
                    throw new Exception("Customer cannot be null when prepopulating an address");
                var obj = await _workContext.GetCurrentCustomerAsync();
                model.Email = obj.Email;
                model.FirstName = await _genericAttributeService.GetAttributeAsync<string>(customer, obj.FirstName);
                model.LastName = await _genericAttributeService.GetAttributeAsync<string>(customer, obj.LastName);
                model.Company = await _genericAttributeService.GetAttributeAsync<string>(customer, obj.Company);
                model.Address1 = await _genericAttributeService.GetAttributeAsync<string>(customer, obj.StreetAddress);
                model.Address2 = await _genericAttributeService.GetAttributeAsync<string>(customer, obj.StreetAddress2);
                model.ZipPostalCode = await _genericAttributeService.GetAttributeAsync<string>(customer, obj.ZipPostalCode);
                model.City = await _genericAttributeService.GetAttributeAsync<string>(customer, obj.City);
                model.County = await _genericAttributeService.GetAttributeAsync<string>(customer, obj.County);
                //ignore country and state for prepopulation. it can cause some issues when posting pack with errors, etc
                //model.CountryId = await _genericAttributeService.GetAttributeAsync<int>(SystemCustomerAttributeNames.CountryId);
                //model.StateProvinceId = await _genericAttributeService.GetAttributeAsync<int>(SystemCustomerAttributeNames.StateProvinceId);
                model.PhoneNumber = await _genericAttributeService.GetAttributeAsync<string>(customer, obj.Phone);
                model.FaxNumber = await _genericAttributeService.GetAttributeAsync<string>(customer, obj.Fax);
            }

            //countries and states
            var countries = await loadCountriesAsync();

            if (_addressSettings.PreselectCountryIfOnlyOne && countries.Count == 1)
            {
                model.CountryId = countries[0].Id;
            }
            else
            {
                model.AvailableCountries.Add(new SelectListItem { Text = await _localizationService.GetResourceAsync("Address.SelectCountry"), Value = "0" });
            }

            foreach (var c in countries)
            {
                model.AvailableCountries.Add(new SelectListItem
                {
                    Text = await _localizationService.GetLocalizedAsync(c, x => x.Name),
                    Value = c.Id.ToString(),
                    Selected = c.Id == model.CountryId
                });
            }

            var languageId = (_workContext.GetWorkingLanguageAsync()).Id;
            var states = (await _stateProvinceService
                .GetStateProvincesByCountryIdAsync(model.CountryId.HasValue ? model.CountryId.Value : 0, languageId))
                .ToList();

            if (states.Any())
            {
                model.AvailableStates.Add(new SelectListItem { Text = await _localizationService.GetResourceAsync("Address.SelectState"), Value = "0" });

                foreach (var s in states)
                {
                    model.AvailableStates.Add(new SelectListItem
                    {
                        Text = await _localizationService.GetLocalizedAsync(s, x => x.Name),
                        Value = s.Id.ToString(),
                        Selected = (s.Id == model.StateProvinceId)
                    });
                }
            }
            else
            {
                var anyCountrySelected = model.AvailableCountries.Any(x => x.Selected);
                model.AvailableStates.Add(new SelectListItem
                {
                    Text = await _localizationService.GetResourceAsync(anyCountrySelected ? "Address.OtherNonUS" : "Address.SelectState"),
                    Value = "0"
                });
            }

            //form fields
            model.CompanyEnabled = true;
            model.CompanyRequired = true;
            model.StreetAddressEnabled = true;
            model.StreetAddressRequired = true;
            model.StreetAddress2Enabled = true;
            model.ZipPostalCodeEnabled = true;
            model.ZipPostalCodeRequired = true;
            model.CityEnabled = true;
            model.CityRequired = true;
            model.CountryEnabled = true;
            model.StateProvinceEnabled = true;
            model.PhoneEnabled = true;
            model.PhoneRequired = true;
            model.FaxEnabled = true;
        }

        #endregion

        #region Methods

        public async Task<AffiliateInfoModel> PrepareAffiliateInfoModelAsync()
        {
            var model = new AffiliateInfoModel();

            var affiliateCustomer = await _affiliateCustomerService.GetAffiliateCustomerByCustomerIdAsync((await _workContext.GetCurrentCustomerAsync()).Id);
            if (affiliateCustomer != null)
            {
                var affiliate = await _affiliateService.GetAffiliateByIdAsync(affiliateCustomer.AffiliateId);

                if (affiliate != null && !affiliate.Deleted)
                {
                    model.AlreadyApplied = true;
                    model.ApplyStatus = affiliateCustomer.ApplyStatus;
                    model.CanUpdateInfo = affiliateCustomer.ApplyStatus != ApplyStatus.Rejected;
                    model.FriendlyUrlName = affiliate.FriendlyUrlName;
                    model.Url = await _affiliateService.GenerateUrlAsync(affiliate);

                    var affiliateAddress = await _addressService.GetAddressByIdAsync(affiliate.AddressId);

                    await PrepareAddressModelAsync(model.Address,
                        address: affiliateAddress,
                        excludeProperties: false,
                        loadCountriesAsync: async () => await _countryService.GetAllCountriesAsync((await _workContext.GetWorkingLanguageAsync()).Id),
                        customer: await _workContext.GetCurrentCustomerAsync());

                    if (affiliateCustomer.ApplyStatus == ApplyStatus.Applied)
                        model.WarningText = await _localizationService.GetResourceAsync("NopStation.AffiliateStation.Account.AppliedWarning");
                    else if (affiliateCustomer.ApplyStatus == ApplyStatus.Rejected)
                        model.WarningText = await _localizationService.GetResourceAsync("NopStation.AffiliateStation.Account.RejectedWarning");
                    else if (!affiliate.Active)
                        model.WarningText = await _localizationService.GetResourceAsync("NopStation.AffiliateStation.Account.NotActiveWarning");
                }
            }

            if (!model.AlreadyApplied)
            {
                await PrepareAddressModelAsync(model.Address,
                        address: null,
                        excludeProperties: true,
                        loadCountriesAsync: async () => await _countryService.GetAllCountriesAsync((_workContext.GetWorkingLanguageAsync()).Id),
                        prePopulateWithCustomerFields: true,
                        customer: await _workContext.GetCurrentCustomerAsync());
            }

            model.Address.CountyEnabled = _customerSettings.CountyEnabled;
            model.Address.CountyRequired = _customerSettings.CountyRequired;

            return model;
        }

        #endregion
    }
}
