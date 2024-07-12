using System.Threading.Tasks;
using NopStation.Plugin.Misc.WebApiNotification.Areas.Admin.Models;
using NopStation.Plugin.Misc.WebApiNotification.Domains;

namespace NopStation.Plugin.Misc.WebApiNotification.Areas.Admin.Factories
{
    public interface IPushNotificationCampaignModelFactory
    {
        WebApiNotificationCampaignSearchModel PreparePushNotificationCampaignSearchModel(WebApiNotificationCampaignSearchModel searchModel);

        Task<WebApiNotificationCampaignListModel> PreparePushNotificationCampaignListModelAsync(WebApiNotificationCampaignSearchModel searchModel);

        Task<WebApiNotificationCampaignModel> PreparePushNotificationCampaignModelAsync(WebApiNotificationCampaignModel model,
            WebApiNotificationCampaign pushNotificationCampaign, bool excludeProperties = false);
    }
}
