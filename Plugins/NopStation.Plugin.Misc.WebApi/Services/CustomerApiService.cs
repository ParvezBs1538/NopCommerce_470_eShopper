using System;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Customers;
using NopStation.Plugin.Misc.WebApi.Extensions;

namespace NopStation.Plugin.Misc.WebApi.Services
{
    public class CustomerApiService : ICustomerApiService
    {
        private readonly ICustomerService _customerService;

        public CustomerApiService(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        public virtual async Task<Customer> InsertDeviceGuestCustomerAsync(string deviceId)
        {
            var customerGuid = HelperExtension.GetGuid(deviceId);
            var customer = new Customer
            {
                CustomerGuid = customerGuid,
                Active = true,
                CreatedOnUtc = DateTime.UtcNow,
                LastActivityDateUtc = DateTime.UtcNow
            };

            //add to 'Guests' role
            var guestRole = await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.GuestsRoleName);
            if (guestRole == null)
                throw new NopException("'Guests' role could not be loaded");

            await _customerService.InsertCustomerAsync(customer);

            await _customerService.AddCustomerRoleMappingAsync(new CustomerCustomerRoleMapping
            {
                CustomerId = customer.Id,
                CustomerRoleId = guestRole.Id
            });

            return customer;
        }
    }
}
