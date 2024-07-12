using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Domains;

namespace NopStation.Plugin.Widgets.ProgressiveWebApp.Services
{
    public class PushNotificationAnnouncementService : IPushNotificationAnnouncementService
    {
        #region Fields

        private readonly IRepository<PushNotificationAnnouncement> _pushNotificationAnnouncementRepository;

        #endregion

        #region Ctor

        public PushNotificationAnnouncementService(
            IRepository<PushNotificationAnnouncement> pushNotificationAnnouncementRepository)
        {
            _pushNotificationAnnouncementRepository = pushNotificationAnnouncementRepository;
        }

        #endregion

        #region Methods

        public async Task DeletePushNotificationAnnouncementAsync(PushNotificationAnnouncement pushNotificationAnnouncement)
        {
            await _pushNotificationAnnouncementRepository.DeleteAsync(pushNotificationAnnouncement);
        }

        public async Task InsertPushNotificationAnnouncementAsync(PushNotificationAnnouncement pushNotificationAnnouncement)
        {
            await _pushNotificationAnnouncementRepository.InsertAsync(pushNotificationAnnouncement);
        }

        public async Task UpdatePushNotificationAnnouncementAsync(PushNotificationAnnouncement pushNotificationAnnouncement)
        {
            await _pushNotificationAnnouncementRepository.UpdateAsync(pushNotificationAnnouncement);
        }

        public async Task<PushNotificationAnnouncement> GetPushNotificationAnnouncementByIdAsync(int pushNotificationAnnouncementId)
        {
            if (pushNotificationAnnouncementId == 0)
                return null;

            return await _pushNotificationAnnouncementRepository.GetByIdAsync(pushNotificationAnnouncementId, cache => default);
        }

        public async Task<IPagedList<PushNotificationAnnouncement>> GetAllPushNotificationAnnouncementsAsync(int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _pushNotificationAnnouncementRepository.Table;

            query = query.OrderByDescending(e => e.Id);

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        #endregion
    }
}
