using System.Threading.Tasks;
using NopStation.Plugin.Widgets.PushNop.Areas.Admin.Models;
using NopStation.Plugin.Widgets.PushNop.Domains;

namespace NopStation.Plugin.Widgets.PushNop.Areas.Admin.Factories
{
    public interface ISmartGroupNotificationModelFactory
    {
        Task<GroupNotificationSearchModel> PrepareSmartGroupNotificationSearchModelAsync(GroupNotificationSearchModel searchModel);

        Task<GroupNotificationListModel> PrepareSmartGroupNotificationListModelAsync(GroupNotificationSearchModel searchModel);

        Task<GroupNotificationModel> PrepareSmartGroupNotificationModelAsync(GroupNotificationModel model,
            SmartGroupNotification smartGroupNotification, bool excludeProperties = false);
    }
}
