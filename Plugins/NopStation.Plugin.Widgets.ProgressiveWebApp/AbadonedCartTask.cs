using System;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Services.Customers;
using Nop.Services.ScheduleTasks;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Services;

namespace NopStation.Plugin.Widgets.ProgressiveWebApp
{
    public class AbadonedCartTask : IScheduleTask
    {
        private readonly IAbandonedCartTrackingService _abandonedCartTrackingService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkflowNotificationService _workflowNotificationService;
        private readonly ICustomerService _customerService;
        private readonly ProgressiveWebAppSettings _progressiveWebAppSettings;

        public AbadonedCartTask(IAbandonedCartTrackingService abandonedCartTrackingService,
            IStoreContext storeContext,
            IWorkflowNotificationService workflowNotificationService,
            ICustomerService customerService,
            ProgressiveWebAppSettings progressiveWebAppSettings)
        {
            _abandonedCartTrackingService = abandonedCartTrackingService;
            _storeContext = storeContext;
            _workflowNotificationService = workflowNotificationService;
            _customerService = customerService;
            _progressiveWebAppSettings = progressiveWebAppSettings;
        }

        public async Task ExecuteAsync()
        {
            var languageId = (await _storeContext.GetCurrentStoreAsync()).DefaultLanguageId;

            var hoursToBeCheckedBefore = _progressiveWebAppSettings.AbandonedCartCheckingOffset * _progressiveWebAppSettings.UnitTypeId;
            var timeTobeCheckedBefore = DateTime.UtcNow.AddHours(-hoursToBeCheckedBefore);

            var abandonedCartTrackings = await _abandonedCartTrackingService.GetAbandonedCartTrackingsToBeQueuedAsync(timeTobeCheckedBefore);

            foreach (var abandonedCartTracking in abandonedCartTrackings)
            {
                var customer = await _customerService.GetCustomerByIdAsync(abandonedCartTracking.CustomerId);

                await _workflowNotificationService.SendAbandonedCartNotificationAsync(customer, languageId);

                abandonedCartTracking.IsQueued = true;
                await _abandonedCartTrackingService.UpdateAbandonedCartTrackingAsync(abandonedCartTracking);
            }
        }
    }
}