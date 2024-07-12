using System.Linq;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Services.Customers;
using Nop.Services.Payments;
using NopStation.Plugin.Misc.Core.Helpers;

namespace NopStation.Plugin.Payments.Affirm.Extensions
{
    public class AffirmExtension
    {
        public static bool PluginActive()
        {
            var addressSettings = NopInstance.Load<AddressSettings>();
            var customerService = NopInstance.Load<ICustomerService>();
            var workContext = NopInstance.Load<IWorkContext>();
            var storeContext = NopInstance.Load<IStoreContext>();
            var paymentPluginManager = NopInstance.Load<IPaymentPluginManager>();

            //filter by country
            var filterByCountryId = 0;
            if (addressSettings.CountryEnabled)
            {
                filterByCountryId = customerService.GetCustomerBillingAddressAsync(workContext.GetCurrentCustomerAsync().Result).Result?.CountryId ?? 0;
            }

            var paymentMethods = paymentPluginManager
                .LoadActivePluginsAsync(workContext.GetCurrentCustomerAsync().Result, storeContext.GetCurrentStore().Id, filterByCountryId)
                .Result;

            var paymentMethod = paymentMethods.FirstOrDefault(pm => pm.PluginDescriptor.SystemName == "NopStation.Plugin.Payments.Affirm");
            return paymentMethod != null;
        }
    }
}
