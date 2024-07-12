using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Widgets.PushNop.Domains;

namespace NopStation.Plugin.Widgets.PushNop.Services
{
    public interface ISmartGroupService
    {
        Task<IPagedList<SmartGroup>> GetAllSmartGroupsAsync(int pageIndex = 0, int pageSize = int.MaxValue);

        Task<IList<SmartGroup>> GetAllSmartGroupAsync();

        Task InsertSmartGroupAsync(SmartGroup smartGroup);

        Task<SmartGroup> GetSmartGroupByIdAsync(int id);

        Task<SmartGroup> GetSmartGroupByNameAsync(string name);

        Task<IEnumerable<string>> SmartGroupAutoCompleteAsync(string name);

        Task UpdateSmartGroupAsync(SmartGroup smartGroup);

        Task DeleteSmartGroupAsync(SmartGroup smartGroup);

        bool GroupNameExists(string name, int id = 0);

        Task InsertSmartGroupConditionAsync(SmartGroupCondition smartGroup);

        Task<SmartGroupCondition> GetSmartGroupConditionByIdAsync(int id);

        Task<IList<SmartGroupCondition>> GetSmartGroupConditionsBySmartGroupIdAsync(int smartGroupId);

        Task UpdateSmartGroupConditionAsync(SmartGroupCondition smartGroupCondition);

        Task DeleteSmartGroupConditionAsync(SmartGroupCondition smartGroupCondition);
    }
}

