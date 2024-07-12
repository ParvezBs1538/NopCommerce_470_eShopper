using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Stores;
using Nop.Core.Events;
using Nop.Data;
using NopStation.Plugin.Widgets.PushNop.Domains;

namespace NopStation.Plugin.Widgets.PushNop.Services
{
    public class SmartGroupNotificationService : ISmartGroupNotificationService
    {
        #region Fields

        private readonly IEventPublisher _eventPublisher;
        private readonly IRepository<SmartGroupNotification> _smartGroupNotificationRepository;
        private readonly CatalogSettings _catalogSettings;
        private readonly IRepository<StoreMapping> _storeMappingRepository;

        #endregion

        #region Ctor

        public SmartGroupNotificationService(IEventPublisher eventPublisher,
            IRepository<SmartGroupNotification> smartGroupNotificationRepository,
            CatalogSettings catalogSettings,
            IRepository<StoreMapping> storeMappingRepository)
        {
            _eventPublisher = eventPublisher;
            _smartGroupNotificationRepository = smartGroupNotificationRepository;
            _catalogSettings = catalogSettings;
            _storeMappingRepository = storeMappingRepository;
        }

        #endregion

        #region Methods

        public virtual async Task<IPagedList<SmartGroupNotification>> GetAllSmartGroupNotificationsAsync(DateTime? searchFrom = null, DateTime? searchTo = null,
            bool? addedToQueueStatus = null, int storeId = 0, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _smartGroupNotificationRepository.Table.Where(x => !x.Deleted);

            if (searchFrom.HasValue)
                query = query.Where(x => x.SendingWillStartOnUtc >= searchFrom.Value);
            if (searchTo.HasValue)
                query = query.Where(x => x.SendingWillStartOnUtc <= searchTo.Value);
            if (addedToQueueStatus.HasValue)
                query = query.Where(x => x.AddedToQueueOnUtc.HasValue == addedToQueueStatus.Value);

            if (storeId > 0 && !_catalogSettings.IgnoreStoreLimitations)
            {
                query = query.Where(x => x.LimitedToStoreId == storeId);
            }

            query = query.OrderByDescending(x => x.Id);
            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        public virtual async Task<SmartGroupNotification> InsertSmartGroupNotificationAsync(SmartGroupNotification campaign)
        {
            await _smartGroupNotificationRepository.InsertAsync(campaign);
            //event notification
            await _eventPublisher.EntityInsertedAsync(campaign);

            return campaign;
        }

        public async Task<SmartGroupNotification> GetSmartGroupNotificationByIdAsync(int id)
        {
            return await _smartGroupNotificationRepository.GetByIdAsync(id, cache => default);
        }

        public async Task UpdateSmartGroupNotificationAsync(SmartGroupNotification campaign)
        {
            await _smartGroupNotificationRepository.UpdateAsync(campaign);
            //event notification
            await _eventPublisher.EntityUpdatedAsync(campaign);
        }

        public async Task DeleteSmartGroupNotificationAsync(SmartGroupNotification campaign)
        {
            campaign.Deleted = true;
            await _smartGroupNotificationRepository.UpdateAsync(campaign);
            await _eventPublisher.EntityDeletedAsync(campaign);
        }

        public async Task<IList<SmartGroupNotification>> GetSmartGroupNotificationsBySmartGroupIdAsync(int smartGroupId)
        {
            if (smartGroupId == 0)
                return new List<SmartGroupNotification>();

            var query = _smartGroupNotificationRepository.Table.Where(x => x.SmartGroupId == smartGroupId);

            return await query.ToListAsync();
        }

        #endregion
    }
}
