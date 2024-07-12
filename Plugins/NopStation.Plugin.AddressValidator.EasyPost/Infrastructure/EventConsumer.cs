using Nop.Core.Domain.Common;
using Nop.Core.Events;
using NopStation.Plugin.AddressValidator.EasyPost.Domains;
using NopStation.Plugin.AddressValidator.EasyPost.Services;
using Nop.Services.Events;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Checkout;
using System.Linq;
using System.Threading.Tasks;

namespace NopStation.Plugin.AddressValidator.EasyPost.Infrastructure
{
    public class EventConsumer :
        IConsumer<EntityInsertedEvent<Address>>,
        IConsumer<EntityUpdatedEvent<Address>>,
        IConsumer<ModelPreparedEvent<BaseNopModel>>
    {
        private readonly IAddressExtensionService _addressExtensionService;

        public EventConsumer(IAddressExtensionService addressExtensionService)
        {
            _addressExtensionService = addressExtensionService;
        }

        public async Task HandleEventAsync(EntityInsertedEvent<Address> eventMessage)
        {
            var address = eventMessage.Entity;

            var response = await _addressExtensionService.GetGeocodingResponseAsync(address.ZipPostalCode, address.Address1, address.Address2,
                address.PhoneNumber, address.City, address.StateProvinceId, address.CountryId, address.Company, address.CustomAttributes);

            if (response.Verification.Delivery.Success)
            {
                var result = response.Verification.Delivery.Details;
                var addressExtension = new EasyPostAddressExtension()
                {
                    AddressId = address.Id,
                    Latitude = result.Latitude.ToString(),
                    Longitude = result.Longitude.ToString(),
                    TimeZone = result.TimeZone
                };
                await _addressExtensionService.InsertAddressExtensionAsync(addressExtension);
            }
        }
        
        public async Task HandleEventAsync(EntityUpdatedEvent<Address> eventMessage)
        {
            var address = eventMessage.Entity;
            var response = await _addressExtensionService.GetGeocodingResponseAsync(address.ZipPostalCode, address.Address1, address.Address2,
                address.PhoneNumber, address.City, address.StateProvinceId, address.CountryId, address.Company, address.CustomAttributes);

            if (response.Verification.Delivery.Success)
            {
                var result = response.Verification.Delivery.Details;
                var addressExtension = await _addressExtensionService.GetAddressExtensionByAddressIdAsync(address.Id);

                if (addressExtension == null)
                {
                    var newAddressExtension = new EasyPostAddressExtension()
                    {
                        AddressId = address.Id,
                        Latitude = result.Latitude.ToString(),
                        Longitude = result.Longitude.ToString(),
                        TimeZone = result.TimeZone
                    };
                    await _addressExtensionService.InsertAddressExtensionAsync(newAddressExtension);
                }
                else
                {
                    addressExtension.Latitude = result.Latitude.ToString();
                    addressExtension.Longitude = result.Longitude.ToString();
                    addressExtension.TimeZone = result.TimeZone;

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
