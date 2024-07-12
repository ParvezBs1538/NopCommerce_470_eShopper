using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Nop.Core.Domain.Customers;
using Nop.Core.Events;
using Nop.Services.Customers;
using Nop.Services.Events;
using NopStation.Plugin.Widgets.DMS.Domain;
using NopStation.Plugin.Widgets.DMS.Services;

namespace NopStation.Plugin.Widgets.DMS.Infrastructure
{
    public class EventConsumer : IConsumer<EntityUpdatedEvent<Customer>>,
        IConsumer<EntityDeletedEvent<Shipper>>,
        IConsumer<CustomerLoggedOutEvent>,
        IConsumer<CustomerLoggedinEvent>
    {
        private readonly IShipperService _shipperService;
        private readonly ICustomerService _customerService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IShipperDeviceService _deviceService;

        public EventConsumer(IShipperService shipperService,
            ICustomerService customerService,
            IHttpContextAccessor httpContextAccessor,
            IShipperDeviceService deviceService
            )
        {
            _shipperService = shipperService;
            _customerService = customerService;
            _httpContextAccessor = httpContextAccessor;
            _deviceService = deviceService;
        }

        public async Task HandleEventAsync(EntityUpdatedEvent<Customer> eventMessage)
        {
            var customerRole = await _customerService.GetCustomerRoleBySystemNameAsync(DMSDefaults.ShipperCustomerRoleName);
            if (customerRole == null)
                return;

            var customer = eventMessage.Entity;
            var cp = await _shipperService.GetShipperByCustomerIdAsync(customer.Id);

            if (await _customerService.IsInCustomerRoleAsync(customer, DMSDefaults.ShipperCustomerRoleName) && cp == null)
            {
                await _shipperService.InsertShipperAsync(new Shipper()
                {
                    Active = true,
                    CreatedOnUtc = DateTime.UtcNow,
                    CustomerId = customer.Id
                });
            }
            else if (!await _customerService.IsInCustomerRoleAsync(customer, DMSDefaults.ShipperCustomerRoleName) && cp != null)
                await _shipperService.DeleteShipperAsync(cp);
        }

        public async Task HandleEventAsync(EntityDeletedEvent<Shipper> eventMessage)
        {
            var customerRole = await _customerService.GetCustomerRoleBySystemNameAsync(DMSDefaults.ShipperCustomerRoleName);
            if (customerRole == null)
                return;

            var customer = await _customerService.GetCustomerByIdAsync(eventMessage.Entity.CustomerId);
            if (customer == null)
                return;

            if (await _customerService.IsInCustomerRoleAsync(customer, DMSDefaults.ShipperCustomerRoleName))
                await _customerService.RemoveCustomerRoleMappingAsync(customer, customerRole);
        }


        public async Task HandleEventAsync(CustomerLoggedOutEvent eventMessage)
        {
            var customer = eventMessage.Customer;
            if (customer == null)
                return;

            if (!await _customerService.IsInCustomerRoleAsync(customer, DMSDefaults.ShipperCustomerRoleName))
                return;

            var device = await _deviceService.GetShipperDeviceByCustomerAsync(customer);
            if (device != null)
            {
                if (_httpContextAccessor.HttpContext.Request.Headers
                    .TryGetValue(DMSDefaults.DeviceId_Attribute, out StringValues headerValues))
                {
                    var deviceId = headerValues.FirstOrDefault();
                    if (string.IsNullOrEmpty(deviceId))
                        return;

                    if (deviceId != device.DeviceToken)
                        return;

                    device.DeviceToken = string.Empty;
                    device.SubscriptionId = string.Empty;
                    device.UpdatedOnUtc = DateTime.UtcNow;

                    await _deviceService.UpdateShipperDeviceAsync(device);
                }
            }
        }

        public async Task HandleEventAsync(CustomerLoggedinEvent eventMessage)
        {
            var customer = eventMessage.Customer;
            if (customer == null)
                return;

            if (!await _customerService.IsInCustomerRoleAsync(customer, DMSDefaults.ShipperCustomerRoleName))
                return;
            var device = await _deviceService.GetShipperDeviceByCustomerAsync(customer);
            if (device != null)
            {

                if (_httpContextAccessor.HttpContext.Request.Headers
                    .TryGetValue(DMSDefaults.DeviceId_Attribute, out StringValues headerValues))
                {
                    var deviceId = headerValues.FirstOrDefault();
                    if (string.IsNullOrEmpty(deviceId))
                        return;
                    device.DeviceToken = deviceId;
                    device.UpdatedOnUtc = DateTime.UtcNow;
                    await _deviceService.UpdateShipperDeviceAsync(device);
                }
            }
        }
    }
}
