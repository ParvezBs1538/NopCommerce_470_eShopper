using System.Collections.Generic;
using Nop.Core.Domain.Customers;

namespace NopStation.Plugin.Widgets.ProgressiveWebApp.Services
{
    public interface IPushNotificationCustomerService
    {
        IList<Customer> GetCustomersByVendorId(int vendorId);
    }
}