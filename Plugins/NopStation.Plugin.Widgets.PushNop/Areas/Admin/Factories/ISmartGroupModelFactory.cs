using System.Threading.Tasks;
using NopStation.Plugin.Widgets.PushNop.Areas.Admin.Models;
using NopStation.Plugin.Widgets.PushNop.Domains;

namespace NopStation.Plugin.Widgets.PushNop.Areas.Admin.Factories
{
    public interface ISmartGroupModelFactory
    {
        Task<SmartGroupSearchModel> PrepareSmartGroupSearchModelAsync(SmartGroupSearchModel searchModel);

        Task<SmartGroupListModel> PrepareSmartGroupListModelAsync(SmartGroupSearchModel searchModel);

        Task<SmartGroupModel> PrepareSmartGroupModelAsync(SmartGroupModel model,
            SmartGroup pushNotificationTemplate, bool excludeProperties = false);

        Task<SmartGroupConditionListModel> PrepareSmartGroupConditionListModelAsync(SmartGroupConditionSearchModel searchModel, SmartGroup smartGroup);

        Task<SmartGroupConditionModel> PrepareSmartGroupConditionModelAsync(SmartGroup smartGroup, SmartGroupConditionModel model,
            SmartGroupCondition smartGroupCondition, bool excludeProperties = false);
    }
}
