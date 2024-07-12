using Nop.Core.Domain.Customers;
using NopStation.Plugin.Misc.Core.Helpers;
using Nop.Services.Customers;

namespace NopStation.Plugin.Widgets.ProductRequests
{
    public static class ProductRequestHelper
    {
        public static bool HasAccessToProductRequest(this Customer customer, ProductRequestSettings productRequestSettings)
        {
            var customerService = NopInstance.Load<ICustomerService>();
            var customerRoles =  customerService.GetCustomerRolesAsync(customer).Result;
            foreach (var customerRole in customerRoles)
            {
                if (productRequestSettings.AllowedCustomerRolesIds.Contains(customerRole.Id))
                    return true;
            }

            return false;
        }
    }  
}
