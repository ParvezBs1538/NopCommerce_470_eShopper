using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Events;
using NopStation.Plugin.AddressValidator.Google.Domains;
using NopStation.Plugin.AddressValidator.Google.Services;
using Nop.Services.Common;
using Nop.Services.Directory;
using Nop.Services.Events;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Checkout;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace NopStation.Plugin.AddressValidator.Google.Infrastructure
{
    public class EventConsumer :
        IConsumer<EntityInsertedEvent<Address>>,
        IConsumer<EntityUpdatedEvent<Address>>,
        IConsumer<ModelPreparedEvent<BaseNopModel>>
    {
        private readonly IAddressService _addressService;
        private readonly IAddressExtensionService _addressExtensionService;
        private readonly ICountryService _countryService;

        public EventConsumer(IAddressService addressService,
            IAddressExtensionService addressExtensionService,
            ICountryService countryService)
        {
            _addressService = addressService;
            _addressExtensionService = addressExtensionService;
            _countryService = countryService;
        }

        public async Task HandleEventAsync(EntityInsertedEvent<Address> eventMessage)
        {
            var address = eventMessage.Entity;

            var response = await _addressExtensionService.GetGeocodingResponseAsync(address.ZipPostalCode, address.Address1, address.City, 
                address.StateProvinceId, address.CountryId, address.CustomAttributes);

            if (response.Status == "OK")
            {
                var result = response.Results[0];
                var addressExtension = new GoogleAddressExtension()
                {
                    AddressId = address.Id,
                    GoogleCompoundCode = result.PlusCode.CompoundCode,
                    GoogleGlobalCode = result.PlusCode.GlobalCode,
                    GooglePlaceId = result.PlaceId,
                    Latitude = result.Geometry.Location.Latitude.ToString(),
                    Longitude = result.Geometry.Location.Longitude.ToString(),
                    LocationType = result.Geometry.LocationType
                };
                await _addressExtensionService.InsertAddressExtensionAsync(addressExtension);
            }
        }
        
        public async Task HandleEventAsync(EntityUpdatedEvent<Address> eventMessage)
        {
            var address = eventMessage.Entity;
            var response = await _addressExtensionService.GetGeocodingResponseAsync(address.ZipPostalCode, address.Address1, address.City,
                address.StateProvinceId, address.CountryId, address.CustomAttributes);

            if (response.Status == "OK")
            {
                var result = response.Results[0];
                var addressExtension = await _addressExtensionService.GetAddressExtensionByAddressIdAsync(address.Id);

                if (addressExtension == null)
                {
                    var newAddressExtension = new GoogleAddressExtension()
                    {
                        AddressId = address.Id,
                        GoogleCompoundCode = result.PlusCode.CompoundCode,
                        GoogleGlobalCode = result.PlusCode.GlobalCode,
                        GooglePlaceId = result.PlaceId,
                        Latitude = result.Geometry.Location.Latitude.ToString(),
                        Longitude = result.Geometry.Location.Longitude.ToString(),
                        LocationType = result.Geometry.LocationType
                    };
                    await _addressExtensionService.InsertAddressExtensionAsync(newAddressExtension);
                }
                else
                {
                    addressExtension.GoogleCompoundCode = result.PlusCode.CompoundCode;
                    addressExtension.GoogleGlobalCode = result.PlusCode.GlobalCode;
                    addressExtension.GooglePlaceId = result.PlaceId;
                    addressExtension.Latitude = result.Geometry.Location.Latitude.ToString();
                    addressExtension.Longitude = result.Geometry.Location.Longitude.ToString();
                    addressExtension.LocationType = result.Geometry.LocationType;

                    await _addressExtensionService.UpdateAddressExtension(addressExtension);
                }
            }
        }

        public async Task HandleEventAsync(ModelPreparedEvent<BaseNopModel> eventMessage)
        {
            if (eventMessage?.Model is CheckoutBillingAddressModel billingAddressModel)
            {
                var ids = billingAddressModel.ExistingAddresses.Select(x => x.Id).ToArray();
                var addressExtensions = await _addressExtensionService.GetAddressExtensionsByAddressIdsAsync(ids);
                var validAddressIds = addressExtensions.Select(x => x.AddressId);

                var invalidAddresses = billingAddressModel.ExistingAddresses.Where(x => !validAddressIds.Contains(x.Id)).ToList();
                foreach (var address in invalidAddresses)
                {
                    var index = billingAddressModel.ExistingAddresses.IndexOf(address);
                    if (index >= 0)
                        billingAddressModel.ExistingAddresses.RemoveAt(index);
                    billingAddressModel.InvalidExistingAddresses.Add(address);
                }
            }

            if (eventMessage?.Model is CheckoutShippingAddressModel shippingAddressModel)
            {
                var ids = shippingAddressModel.ExistingAddresses.Select(x => x.Id).ToArray();
                var addressExtensions = await _addressExtensionService.GetAddressExtensionsByAddressIdsAsync(ids);
                var validAddressIds = addressExtensions.Select(x => x.AddressId);

                var invalidAddresses = shippingAddressModel.ExistingAddresses.Where(x => !validAddressIds.Contains(x.Id)).ToList();
                foreach (var address in invalidAddresses)
                {
                    var index = shippingAddressModel.ExistingAddresses.IndexOf(address);
                    if (index >= 0)
                        shippingAddressModel.ExistingAddresses.RemoveAt(index);
                    shippingAddressModel.InvalidExistingAddresses.Add(address);
                }
            }
        }
    }
}
