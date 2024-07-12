using System.Threading.Tasks;
using NopStation.Plugin.Misc.WebApiNotification.Areas.Admin.Models;
using NopStation.Plugin.Misc.WebApiNotification.Domains;

namespace NopStation.Plugin.Misc.WebApiNotification.Areas.Admin.Factories
{
    public interface IPushNotificationTemplateModelFactory
    {
        Task<WebApiNotificationTemplateSearchModel> PreparePushNotificationTemplateSearchModelAsync(WebApiNotificationTemplateSearchModel searchModel);

        Task<WebApiNotificationTemplateListModel> PreparePushNotificationTemplateListModelAsync(WebApiNotificationTemplateSearchModel searchModel);

        Task<WebApiNotificationTemplateModel> PreparePushNotificationTemplateModelAsync(WebApiNotificationTemplateModel model,
            WebApiNotificationTemplate pushNotificationTemplate, bool excludeProperties = false);
    }
}