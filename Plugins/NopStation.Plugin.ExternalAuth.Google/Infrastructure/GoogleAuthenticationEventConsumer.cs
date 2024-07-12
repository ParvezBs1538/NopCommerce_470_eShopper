using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Nop.Services.Authentication.External;
using Nop.Services.Customers;
using Nop.Services.Events;

namespace NopStation.Plugin.ExternalAuth.Google.Infrastructure
{
    public class GoogleAuthenticationEventConsumer : IConsumer<CustomerAutoRegisteredByExternalMethodEvent>
    {
        #region Fields

        private readonly ICustomerService _customerService;

        #endregion

        #region Ctor

        public GoogleAuthenticationEventConsumer(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        #endregion

        #region Methods

        public async Task HandleEventAsync(CustomerAutoRegisteredByExternalMethodEvent eventMessage)
        {
            if (eventMessage?.Customer == null || eventMessage.AuthenticationParameters == null)
                return;

            //handle event only for this authentication method
            if (!eventMessage.AuthenticationParameters.ProviderSystemName.Equals(GoogleAuthenticationDefaults.SystemName))
                return;

            var customer = eventMessage.Customer;
            //store some of the customer fields
            var firstName = eventMessage.AuthenticationParameters.Claims?.FirstOrDefault(claim => claim.Type == ClaimTypes.GivenName)?.Value;
            if (!string.IsNullOrEmpty(firstName))
                customer.FirstName = firstName;

            var lastName = eventMessage.AuthenticationParameters.Claims?.FirstOrDefault(claim => claim.Type == ClaimTypes.Surname)?.Value;
            if (!string.IsNullOrEmpty(lastName))
                customer.LastName = lastName;

            await _customerService.UpdateCustomerAsync(customer);
        }

        #endregion
    }
}