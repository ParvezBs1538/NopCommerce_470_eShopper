using System.Threading.Tasks;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Areas.Admin.Models;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Domains;

namespace NopStation.Plugin.Widgets.ProgressiveWebApp.Areas.Admin.Factories
{
    public interface IPushNotificationAnnouncementModelFactory
    {
        PushNotificationAnnouncementSearchModel PreparePushNotificationAnnouncementSearchModel(PushNotificationAnnouncementSearchModel searchModel);

        Task<PushNotificationAnnouncementListModel> PreparePushNotificationAnnouncementListModelAsync(PushNotificationAnnouncementSearchModel searchModel);

        Task<PushNotificationAnnouncementModel> PreparePushNotificationAnnouncementModelAsync(PushNotificationAnnouncementModel model,
            PushNotificationAnnouncement pushNotificationAnnouncement, bool excludeProperties = false);
    }
}