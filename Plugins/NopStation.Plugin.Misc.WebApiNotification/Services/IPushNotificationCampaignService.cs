using System;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Misc.WebApi.Domains;
using NopStation.Plugin.Misc.WebApiNotification.Domains;

namespace NopStation.Plugin.Misc.WebApiNotification.Services
{
    public interface IPushNotificationCampaignService
    {
        Task<IPagedList<WebApiNotificationCampaign>> GetAllPushNotificationCampaignsAsync(string keyword = "",
            DateTime? searchFrom = null, DateTime? searchTo = null, bool? addedToQueueStatus = null,
            int storeId = 0, int pageIndex = 0, int pageSize = int.MaxValue);

        Task InsertPushNotificationCampaignAsync(WebApiNotificationCampaign campaign);

        Task<WebApiNotificationCampaign> GetPushNotificationCampaignByIdAsync(int id);

        Task UpdatePushNotificationCampaignAsync(WebApiNotificationCampaign campaign);

        Task DeletePushNotificationCampaignAsync(WebApiNotificationCampaign campaign);

        Task<IPagedList<ApiDevice>> GetCampaignDevicesAsync(WebApiNotificationCampaign campaign, int pageIndex = 0, int pageSize = 100);
    }
}
