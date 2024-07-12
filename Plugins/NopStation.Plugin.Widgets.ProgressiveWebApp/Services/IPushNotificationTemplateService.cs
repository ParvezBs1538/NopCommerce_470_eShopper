using System.Collections.Generic;
using System.Threading.Tasks;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Domains;

namespace NopStation.Plugin.Widgets.ProgressiveWebApp.Services
{
    public interface IPushNotificationTemplateService
    {
        Task DeletePushNotificationTemplateAsync(PushNotificationTemplate pushNotificationTemplate);

        Task InsertPushNotificationTemplateAsync(PushNotificationTemplate pushNotificationTemplate);

        Task UpdatePushNotificationTemplateAsync(PushNotificationTemplate pushNotificationTemplate);

        Task<PushNotificationTemplate> GetPushNotificationTemplateByIdAsync(int pushNotificationTemplateId);

        Task<IList<PushNotificationTemplate>> GetAllPushNotificationTemplatesAsync(int storeId);

        Task<IList<PushNotificationTemplate>> GetPushNotificationTemplatesByNameAsync(string messageTemplateName, int? storeId = null);
    }
}