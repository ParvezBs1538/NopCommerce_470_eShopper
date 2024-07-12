using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Events;
using NopStation.Plugin.AddressValidator.Byteplant.Domains;
using NopStation.Plugin.AddressValidator.Byteplant.Services;
using Nop.Services.Common;
using Nop.Services.Directory;
using Nop.Services.Events;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Checkout;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace NopStation.Plugin.AddressValidator.Byteplant.Infrastructure
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

            if (response.Status == "VALID")
            {
                var addressExtension = new ByteplantAddressExtension()
                {
                    AddressId = address.Id,
                    FormattedAddress = response.FormattedAddress
                };
                await _addressExtensionService.InsertAddressExtensionAsync(addressExtension);
            }
        }
        
        public async Task HandleEventAsync(EntityUpdatedEvent<Address> eventMessage)
        {
            var address = eventMessage.Entity;
            var response = await _addressExtensionService.GetGeocodingResponseAsync(address.ZipPostalCode, address.Address1, address.City,
                address.StateProvinceId, address.CountryId, address.CustomAttributes);

            if (response.Status == "VALID")
            {
                var addressExtension = await _addressExtensionService.GetAddressExtensionByAddressIdAsync(address.Id);

                if (addressExtension == null)
                {
                    var newAddressExtension = new ByteplantAddressExtension()
                    {
                        AddressId = address.Id,
                        FormattedAddress = response.FormattedAddress
                    };
                    await _addressExtensionService.InsertAddressExtensionAsync(newAddressExtension);
                }
                else
                {
                    addressExtension.FormattedAddress = response.FormattedAddress;

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
