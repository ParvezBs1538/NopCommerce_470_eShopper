using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Domains;

namespace NopStation.Plugin.Widgets.ProgressiveWebApp.Services
{
    public class QueuedPushNotificationService : IQueuedPushNotificationService
    {
        #region Fields

        private readonly IRepository<QueuedPushNotification> _queuedPushNotificationRepository;

        #endregion

        #region Ctor

        public QueuedPushNotificationService(
            IRepository<QueuedPushNotification> queuedPushNotificationRepository)
        {
            _queuedPushNotificationRepository = queuedPushNotificationRepository;
        }

        #endregion

        #region Methods

        public async Task DeleteQueuedPushNotificationAsync(QueuedPushNotification queuedPushNotification)
        {
            await _queuedPushNotificationRepository.DeleteAsync(queuedPushNotification);
        }

        public async Task InsertQueuedPushNotificationAsync(QueuedPushNotification queuedPushNotification)
        {
            await _queuedPushNotificationRepository.InsertAsync(queuedPushNotification);
        }

        public async Task UpdateQueuedPushNotificationAsync(QueuedPushNotification queuedPushNotification)
        {
            await _queuedPushNotificationRepository.UpdateAsync(queuedPushNotification);
        }

        public async Task<QueuedPushNotification> GetQueuedPushNotificationByIdAsync(int queuedPushNotificationId)
        {
            if (queuedPushNotificationId == 0)
                return null;

            return await _queuedPushNotificationRepository.GetByIdAsync(queuedPushNotificationId, cache => default);
        }

        public async Task<IPagedList<QueuedPushNotification>> GetAllQueuedPushNotificationsAsync(bool? sentItems = null, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _queuedPushNotificationRepository.Table;

            if (sentItems.HasValue)
                query = query.Where(qe => qe.SentOnUtc.HasValue == sentItems.Value);

            query = query.OrderByDescending(e => e.Id);

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        #endregion
    }
}
