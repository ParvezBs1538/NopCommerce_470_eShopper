using System;
using System.Threading.Tasks;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Services;
using NopStation.Plugin.Widgets.PushNop.Areas.Admin.Models;
using NopStation.Plugin.Widgets.PushNop.Services;

namespace NopStation.Plugin.Widgets.PushNop.Areas.Admin.Factories
{
    public class PushNotificationHomeFactory : IPushNotificationHomeFactory
    {
        #region Fields

        private readonly IWebAppDeviceService _webAppDeviceService;
        private readonly IQueuedPushNotificationService _queuedPushNotificationService;
        private readonly ISmartGroupNotificationService _smartGroupNotificationService;

        #endregion

        public PushNotificationHomeFactory(IWebAppDeviceService webAppDeviceService,
            IQueuedPushNotificationService queuedPushNotificationService,
            ISmartGroupNotificationService smartGroupNotificationService)
        {
            _webAppDeviceService = webAppDeviceService;
            _queuedPushNotificationService = queuedPushNotificationService;
            _smartGroupNotificationService = smartGroupNotificationService;
        }

        public virtual async Task<PushNopDashbordModel> PreparePushNotificationDashboardModelAsync(PushNopDashbordModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            model.NumberOfSubscribers = (await _webAppDeviceService
                .GetWebAppDevicesAsync(pageIndex: 0, pageSize: 1)).TotalCount;

            model.NumberOfNotifications = (await _queuedPushNotificationService
                .GetAllQueuedPushNotificationsAsync(sentItems: false, pageIndex: 0, pageSize: 1)).TotalCount;

            model.NumberOfCampaignsSent = (await _smartGroupNotificationService
                .GetAllSmartGroupNotificationsAsync(addedToQueueStatus: true, pageIndex: 0, pageSize: 1)).TotalCount;

            model.NumberOfCampaignsScheduled = (await _smartGroupNotificationService
                .GetAllSmartGroupNotificationsAsync(pageIndex: 0, pageSize: 1)).TotalCount;

            model.NumberOfNewSubscribersByDay = (await _webAppDeviceService
                .GetWebAppDevicesAsync(createdFromUtc: DateTime.UtcNow.AddDays(-1), pageIndex: 0, pageSize: 1)).TotalCount;

            model.NumberOfNewSubscribersByWeek = (await _webAppDeviceService
                .GetWebAppDevicesAsync(createdFromUtc: DateTime.UtcNow.AddDays(-7), pageIndex: 0, pageSize: 1)).TotalCount;

            model.NumberOfNewSubscribersByMonth = (await _webAppDeviceService
                .GetWebAppDevicesAsync(createdFromUtc: DateTime.UtcNow.AddMonths(-1), pageIndex: 0, pageSize: 1)).TotalCount;

            model.NumberOfNewSubscribersByYear = (await _webAppDeviceService
                .GetWebAppDevicesAsync(createdFromUtc: DateTime.UtcNow.AddMonths(-12), pageIndex: 0, pageSize: 1)).TotalCount;

            return model;
        }
    }
}
