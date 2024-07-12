using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Misc.WebApiNotification.Domains;

namespace NopStation.Plugin.Misc.WebApiNotification.Services
{
    public interface IPushNotificationTemplateService
    {
        Task DeletePushNotificationTemplateAsync(WebApiNotificationTemplate pushNotificationTemplate);

        Task InsertPushNotificationTemplateAsync(WebApiNotificationTemplate pushNotificationTemplate);

        Task UpdatePushNotificationTemplateAsync(WebApiNotificationTemplate pushNotificationTemplate);

        Task<WebApiNotificationTemplate> GetPushNotificationTemplateByIdAsync(int pushNotificationTemplateId);

        Task<IPagedList<WebApiNotificationTemplate>> GetAllPushNotificationTemplatesAsync(string keyword = null, bool? active = null,
            int storeId = 0, int pageIndex = 0, int pageSize = int.MaxValue);

        Task<IList<WebApiNotificationTemplate>> GetPushNotificationTemplatesByNameAsync(string messageTemplateName, int storeId = 0);

        IList<WebApiNotificationTemplate> GetTemplatesByIds(int[] templateIds);
    }
}