using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using NopStation.Plugin.Widgets.BulkDeleteCustomer.Models;
using NopStation.Plugin.Misc.Core.Controllers;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Stores;

namespace NopStation.Plugin.Widgets.BulkDeleteCustomer.Controllers
{
    public class BulkDeleteCustomerController : NopStationAdminController
    {
        #region Fields

        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerService _customerService;
        private readonly ILocalizationService _localizationService;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
        private readonly IPermissionService _permissionService;
        private readonly IStoreService _storeService;
        private readonly IWorkContext _workContext;
        private readonly ILogger _logger;

        #endregion

        #region Ctor

        public BulkDeleteCustomerController(
            ICustomerActivityService customerActivityService,
            ICustomerService customerService,
            ILocalizationService localizationService,
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            IPermissionService permissionService,
            IStoreService storeService,
            IWorkContext workContext,
            ILogger logger)
        {
            _customerActivityService = customerActivityService;
            _customerService = customerService;
            _localizationService = localizationService;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
            _permissionService = permissionService;
            _storeService = storeService;
            _workContext = workContext;
            _logger = logger;
        }

        #endregion

        private async Task<bool> SecondAdminAccountExists(Customer customer)
        {
            var customers = await _customerService.GetAllCustomersAsync(customerRoleIds: new[] { (await _customerService.GetCustomerRoleBySystemNameAsync(NopCustomerDefaults.AdministratorsRoleName)).Id });

            return customers.Any(c => c.Active && c.Id != customer.Id);
        }

        [HttpPost]
        public virtual async Task<IActionResult> Delete(BulkDeleteModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageCustomers))
                return AccessDeniedView();

            var total = 0;
            //try to get a customer with the specified id
            var customers = await _customerService.GetCustomersByIdsAsync(model.CustomerIds);
            if (customers.Any())
            {
                foreach (var customer in customers)
                {
                    try
                    {
                        //prevent attempts to delete the user, if it is the last active administrator
                        if (await _customerService.IsAdminAsync(customer) && !await SecondAdminAccountExists(customer))
                            continue;

                        //ensure that the current customer cannot delete "Administrators" if he's not an admin himself
                        if (await _customerService.IsAdminAsync(customer) && !await _customerService.IsAdminAsync(await _workContext.GetCurrentCustomerAsync()))
                            continue;

                        //delete
                        await _customerService.DeleteCustomerAsync(customer);

                        //remove newsletter subscription (if exists)
                        foreach (var store in await _storeService.GetAllStoresAsync())
                        {
                            var subscription = await _newsLetterSubscriptionService.GetNewsLetterSubscriptionByEmailAndStoreIdAsync(customer.Email, store.Id);
                            if (subscription != null)
                                await _newsLetterSubscriptionService.DeleteNewsLetterSubscriptionAsync(subscription);
                        }

                        //activity log
                        await _customerActivityService.InsertActivityAsync("DeleteCustomer",
                            string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteCustomer"), customer.Id), customer);

                        total++;
                    }
                    catch (Exception exc)
                    {
                        await _logger.ErrorAsync(exc.Message, exc);
                    }
                }
            }

            return Json(new {
                Result = true, 
                Message = string.Format(await _localizationService.GetResourceAsync("Admin.NopStation.BulkDeleteCustomer.CustomersDeleted"), total) 
            });
        }
    }
}
