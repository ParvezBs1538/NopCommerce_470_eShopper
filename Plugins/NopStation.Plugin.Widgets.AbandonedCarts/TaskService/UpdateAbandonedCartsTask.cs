using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using Nop.Services.ScheduleTasks;
using NopStation.Plugin.Widgets.AbandonedCarts.Domain;
using NopStation.Plugin.Widgets.AbandonedCarts.Factories;
using NopStation.Plugin.Widgets.AbandonedCarts.Models;
using NopStation.Plugin.Widgets.AbandonedCarts.Services;
using NopStation.Plugin.Widgets.AbandonedCarts.Services.Messages;
using NUglify.Helpers;

namespace NopStation.Plugin.Widgets.AbandonedCarts.TaskService
{
    public class UpdateAbandonedCartsTask : IScheduleTask
    {
        #region Fields

        private readonly ILogger _logger;
        private readonly IAbandonedCartService _abandonedCartService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IAbandonedCartMessageService _abandonedCartMessageService;
        private readonly IAbandonedCartFactory _abandonedCartFactory;
        private readonly ICustomerAbandonmentInfoService _customerAbandonmentInfoService;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctors

        public UpdateAbandonedCartsTask(IAbandonedCartService abandonedCartService,
            ISettingService settingService,
            IStoreContext storeContext,
            ILogger logger,
            IAbandonedCartMessageService abandonedCartMessageService,
            IAbandonedCartFactory abandonedCartFactory,
            ICustomerAbandonmentInfoService customerAbandonmentInfoService,
            IJwtTokenService jwtTokenService,
            IWorkContext workContext)
        {
            _abandonedCartService = abandonedCartService;
            _settingService = settingService;
            _storeContext = storeContext;
            _logger = logger;
            _abandonedCartMessageService = abandonedCartMessageService;
            _abandonedCartFactory = abandonedCartFactory;
            _customerAbandonmentInfoService = customerAbandonmentInfoService;
            _jwtTokenService = jwtTokenService;
            _workContext = workContext;
        }

        #endregion

        #region Methods

        public async Task ExecuteAsync()
        {
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var abandonedCartsSettings = await _settingService.LoadSettingAsync<AbandonmentCartSettings>(storeScope);

            await _abandonedCartService.UpdateAbandonmentStatusAsync();

            if (abandonedCartsSettings.IsEnabled && abandonedCartsSettings.IsEnabledSecondNotification)
                await SendEmailForSecondAbandonedCartsAsync(abandonedCartsSettings);

            if (abandonedCartsSettings.IsEnabled && abandonedCartsSettings.IsEnabledFirstNotification)
                await SendEmailForFirstAbandonedCartsAsync(abandonedCartsSettings);
        }

        private async Task SendEmailForFirstAbandonedCartsAsync(AbandonmentCartSettings setting)
        {
            var customers = await _abandonedCartService.GetFirstAbandonedCustomersAsync();
            if (customers.Count() == 0)
            {
                await _logger.InformationAsync("(Abandoned Carts Plugin) There is no Abandoned carts to send Email for first abandonment!");
                return;
            }
            var mailCount = 0;
            foreach (var customer in customers)
            {
                var lastAbd = await _abandonedCartService.GetLastInactiveAbandonedCartByCustomerAsync(customer.Id);
                if (setting.NotificationSendingConditionId == AbandonedType.AllAbandoned && lastAbd != null)
                    continue;

                //products
                var products = await _abandonedCartFactory.PrepareProductInfoModelsByCustomerAsync(customer.Id);
                // JWT Token
                var jwtToken = _jwtTokenService.GenerateJSONWebToken(customer);

                //so we got the customer, JwtToken and the products. Now just send an email.
                await _abandonedCartMessageService.SendCustomerEmailAsync(customer, products, jwtToken, (await _workContext.GetWorkingLanguageAsync()).Id);
                mailCount++;

                var customerAbandonment = await _customerAbandonmentInfoService.GetCustomerAbandonmentByCustomerIdAsync(customer.Id);

                if (customerAbandonment == null)
                    customerAbandonment = new CustomerAbandonmentInfoModel();

                customerAbandonment.CustomerId = customer.Id;
                customerAbandonment.NotificationSentFrequency += 1;
                customerAbandonment.Token = jwtToken;
                customerAbandonment.LastNotificationSentOn = DateTime.UtcNow;

                await _customerAbandonmentInfoService.AddOrUpdateCustomerAbandonmentAsync(customerAbandonment);

                var abandonedCarts = await _abandonedCartService.GetAbandonedCartsByCustomerIdAsync(customer.Id);

                abandonedCarts.ForEach(c => c.FirstNotificationSentOn = DateTime.UtcNow);

                await _abandonedCartService.BulkUpdateAbandonmentCarts(abandonedCarts);
            }
            if (mailCount > 0)
                await _logger.InformationAsync("(Abandoned Carts Plugin) Mail queued successfully to send for first Abandonment");
        }

        private async Task SendEmailForSecondAbandonedCartsAsync(AbandonmentCartSettings setting)
        {
            var customers = await _abandonedCartService.GetSecondAbandonedCustomersAsync();
            if (customers.Count() == 0)
            {
                await _logger.InformationAsync("(Abandoned Carts Plugin) There is no Abandoned carts to send Email for second abandonment!");
                return;
            }
            var mailCount = 0;
            foreach (var customer in customers)
            {
                var lastAbd = await _abandonedCartService.GetLastInactiveAbandonedCartByCustomerAsync(customer.Id);
                if (setting.NotificationSendingConditionId == AbandonedType.AllAbandoned && lastAbd != null)
                    continue;

                //products
                var products = await _abandonedCartFactory.PrepareProductInfoModelsByCustomerAsync(customer.Id);

                // JWT Token
                var jwtToken = _jwtTokenService.GenerateJSONWebToken(customer);

                //so we got the customer, JwtToken and the products. Now just send an email.
                await _abandonedCartMessageService.SendCustomerEmailAsync(customer, products, jwtToken, (await _workContext.GetWorkingLanguageAsync()).Id);
                mailCount++;

                var customerAbandonment = await _customerAbandonmentInfoService.GetCustomerAbandonmentByCustomerIdAsync(customer.Id);

                if (customerAbandonment == null)
                    customerAbandonment = new CustomerAbandonmentInfoModel();
                customerAbandonment.CustomerId = customer.Id;
                customerAbandonment.NotificationSentFrequency += 1;
                customerAbandonment.LastNotificationSentOn = DateTime.UtcNow;
                customerAbandonment.Token = jwtToken;

                await _customerAbandonmentInfoService.AddOrUpdateCustomerAbandonmentAsync(customerAbandonment);

                var abandonedCarts = await _abandonedCartService.GetAbandonedCartsByCustomerIdAsync(customer.Id);

                abandonedCarts.ForEach(c => c.SecondNotificationSentOn = DateTime.UtcNow);

                await _abandonedCartService.BulkUpdateAbandonmentCarts(abandonedCarts);
            }
            if (mailCount > 0)
                await _logger.InformationAsync("(Abandoned Carts Plugin) Mail queued successfully to send for second Abandonment.");
        }

        #endregion
    }
}
