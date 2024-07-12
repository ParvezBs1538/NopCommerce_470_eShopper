using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Data;
using NopStation.Plugin.Misc.Core.Caching;
using NopStation.Plugin.Misc.WebApiNotification.Domains;

namespace NopStation.Plugin.Misc.WebApiNotification.Services
{
    public class QueuedPushNotificationService : IQueuedPushNotificationService
    {
        #region Fields

        private readonly INopDataProvider _dataProvider;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IRepository<WebApiQueuedNotification> _queuedPushNotificationRepository;

        #endregion

        #region Ctor

        public QueuedPushNotificationService(INopDataProvider dataProvider,
            IStaticCacheManager staticCacheManager,
            IRepository<WebApiQueuedNotification> queuedPushNotificationRepository)
        {
            _dataProvider = dataProvider;
            _staticCacheManager = staticCacheManager;
            _queuedPushNotificationRepository = queuedPushNotificationRepository;
        }

        #endregion

        #region Methods

        public async Task DeleteQueuedPushNotificationAsync(WebApiQueuedNotification queuedPushNotification)
        {
            await _queuedPushNotificationRepository.DeleteAsync(queuedPushNotification);
        }

        public async Task DeleteSentQueuedPushNotificationAsync()
        {
            var query = await _queuedPushNotificationRepository.Table.Where(qe => qe.SentOnUtc.HasValue).ToListAsync();

            await _queuedPushNotificationRepository.DeleteAsync(query);
        }

        public async Task InsertQueuedPushNotificationAsync(WebApiQueuedNotification queuedPushNotification)
        {
            await _queuedPushNotificationRepository.InsertAsync(queuedPushNotification);
        }

        public async Task UpdateQueuedPushNotificationAsync(WebApiQueuedNotification queuedPushNotification)
        {
            await _queuedPushNotificationRepository.UpdateAsync(queuedPushNotification);
        }

        public async Task<WebApiQueuedNotification> GetQueuedPushNotificationByIdAsync(int queuedPushNotificationId)
        {
            if (queuedPushNotificationId == 0)
                return null;

            return await _queuedPushNotificationRepository.GetByIdAsync(queuedPushNotificationId, cache =>
                _staticCacheManager.PrepareKeyForDefaultCache(NopStationEntityCacheDefaults<WebApiQueuedNotification>.ByIdCacheKey, queuedPushNotificationId));
        }

        public async Task<IPagedList<WebApiQueuedNotification>> GetAllQueuedPushNotificationsAsync(bool? sentStatus = null,
            bool enableDateConsideration = false, DateTime? sentFromUtc = null, DateTime? sentToUtc = null,
            int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _queuedPushNotificationRepository.Table;

            if (sentStatus.HasValue)
                query = query.Where(qe => qe.SentOnUtc.HasValue == sentStatus.Value);

            if (enableDateConsideration)
            {
                query = query.Where(x => x.SentTries < 3);
                query = query.Where(e => !e.DontSendBeforeDateUtc.HasValue || e.DontSendBeforeDateUtc < DateTime.UtcNow);
            }

            if (sentFromUtc.HasValue)
                query = query.Where(e => e.SentOnUtc >= sentFromUtc);

            if (sentToUtc.HasValue)
                query = query.Where(e => e.SentOnUtc <= sentToUtc);

            query = query.OrderByDescending(e => e.Id);

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        #endregion
    }
}