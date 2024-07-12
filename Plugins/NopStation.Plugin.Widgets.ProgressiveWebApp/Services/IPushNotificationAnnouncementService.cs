using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Domains;

namespace NopStation.Plugin.Widgets.ProgressiveWebApp.Services
{
    public interface IPushNotificationAnnouncementService
    {
        Task DeletePushNotificationAnnouncementAsync(PushNotificationAnnouncement pushNotificationAnnouncement);

        Task InsertPushNotificationAnnouncementAsync(PushNotificationAnnouncement pushNotificationAnnouncement);

        Task UpdatePushNotificationAnnouncementAsync(PushNotificationAnnouncement pushNotificationAnnouncement);

        Task<PushNotificationAnnouncement> GetPushNotificationAnnouncementByIdAsync(int pushNotificationAnnouncementId);

        Task<IPagedList<PushNotificationAnnouncement>> GetAllPushNotificationAnnouncementsAsync(int pageIndex = 0, int pageSize = int.MaxValue);
    }
}