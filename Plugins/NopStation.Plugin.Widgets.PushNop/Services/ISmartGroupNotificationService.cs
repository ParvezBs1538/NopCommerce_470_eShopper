using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Widgets.PushNop.Domains;

namespace NopStation.Plugin.Widgets.PushNop.Services
{
    public interface ISmartGroupNotificationService
    {
        Task<IPagedList<SmartGroupNotification>> GetAllSmartGroupNotificationsAsync(DateTime? searchFrom = null, DateTime? searchTo = null,
            bool? addedToQueueStatus = null, int storeId = 0, int pageIndex = 0, int pageSize = int.MaxValue);

        Task<SmartGroupNotification> InsertSmartGroupNotificationAsync(SmartGroupNotification campaign);

        Task<SmartGroupNotification> GetSmartGroupNotificationByIdAsync(int id);

        Task UpdateSmartGroupNotificationAsync(SmartGroupNotification campaign);

        Task DeleteSmartGroupNotificationAsync(SmartGroupNotification campaign);

        Task<IList<SmartGroupNotification>> GetSmartGroupNotificationsBySmartGroupIdAsync(int smartGroupId);
    }
}
