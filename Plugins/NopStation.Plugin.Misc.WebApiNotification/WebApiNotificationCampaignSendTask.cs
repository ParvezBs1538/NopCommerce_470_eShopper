using System;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Services.ScheduleTasks;
using NopStation.Plugin.Misc.WebApiNotification.Services;

namespace NopStation.Plugin.Misc.WebApiNotification
{
    public class WebApiNotificationCampaignSendTask : IScheduleTask
    {
        private readonly IPushNotificationCampaignService _pushNotificationCampaignService;
        private readonly IWorkflowNotificationService _workflowNotificationService;
        private readonly IStoreContext _storeContext;

        public WebApiNotificationCampaignSendTask(IPushNotificationCampaignService pushNotificationCampaignService,
            IWorkflowNotificationService workflowNotificationService,
            IStoreContext storeContext)
        {
            _pushNotificationCampaignService = pushNotificationCampaignService;
            _workflowNotificationService = workflowNotificationService;
            _storeContext = storeContext;
        }

        public async Task ExecuteAsync()
        {
            var campaigns = await _pushNotificationCampaignService.GetAllPushNotificationCampaignsAsync(
                searchTo: DateTime.UtcNow,
                addedToQueueStatus: false,
                storeId: (await _storeContext.GetCurrentStoreAsync()).Id);

            for (int i = 0; i < campaigns.Count; i++)
            {
                var campaign = campaigns[i];
                campaign.AddedToQueueOnUtc = DateTime.UtcNow;
                await _pushNotificationCampaignService.UpdatePushNotificationCampaignAsync(campaign);

                await _workflowNotificationService.SendCampaignNotificationAsync(campaign);
            }
        }
    }
}


