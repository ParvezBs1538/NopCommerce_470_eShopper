using System.Threading.Tasks;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Areas.Admin.Models;
using NopStation.Plugin.Widgets.ProgressiveWebApp.Domains;

namespace NopStation.Plugin.Widgets.ProgressiveWebApp.Areas.Admin.Factories
{
    public interface IPushNotificationTemplateModelFactory
    {
        PushNotificationTemplateSearchModel PreparePushNotificationTemplateSearchModel(PushNotificationTemplateSearchModel searchModel);

        Task<PushNotificationTemplateListModel> PreparePushNotificationTemplateListModelAsync(PushNotificationTemplateSearchModel searchModel);

        Task<PushNotificationTemplateModel> PreparePushNotificationTemplateModelAsync(PushNotificationTemplateModel model,
            PushNotificationTemplate pushNotificationTemplate, bool excludeProperties = false);
    }
}