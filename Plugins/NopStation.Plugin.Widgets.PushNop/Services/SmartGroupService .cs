using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using NopStation.Plugin.Widgets.PushNop.Domains;

namespace NopStation.Plugin.Widgets.PushNop.Services
{
    public class SmartGroupService : ISmartGroupService
    {
        #region Fields

        private readonly IRepository<SmartGroup> _smartGroupsRepository;
        private readonly IRepository<SmartGroupCondition> _smartgroupConditionRepository;

        #endregion

        #region ctor

        public SmartGroupService(IRepository<SmartGroup> smartGroupsRepository,
            IRepository<SmartGroupCondition> smartgroupConditionRepository)
        {
            _smartGroupsRepository = smartGroupsRepository;
            _smartgroupConditionRepository = smartgroupConditionRepository;
        }

        #endregion

        #region Methods

        public virtual async Task<IPagedList<SmartGroup>> GetAllSmartGroupsAsync(int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = from u in _smartGroupsRepository.Table
                        where !u.Deleted
                        orderby u.Id
                        select u;

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        public virtual async Task<IList<SmartGroup>> GetAllSmartGroupAsync()
        {
            var query = from u in _smartGroupsRepository.Table
                        where !u.Deleted
                        orderby u.Id
                        select u;

            return await query.ToListAsync();
        }

        public virtual async Task InsertSmartGroupAsync(SmartGroup smartGroup)
        {
            await _smartGroupsRepository.InsertAsync(smartGroup);
        }

        public async Task<SmartGroup> GetSmartGroupByIdAsync(int id)
        {
            return await _smartGroupsRepository.GetByIdAsync(id, cache => default);
        }

        public virtual async Task<SmartGroup> GetSmartGroupByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            return await _smartGroupsRepository.Table.FirstOrDefaultAsync(x => x.Name.Contains(name) && !x.Deleted);
        }

        public virtual async Task<IEnumerable<string>> SmartGroupAutoCompleteAsync(string name)
        {
            var query = from sg in _smartGroupsRepository.Table
                        where sg.Name.Contains(name) && !sg.Deleted
                        select sg.Name;

            return await query.ToListAsync();
        }

        public async Task UpdateSmartGroupAsync(SmartGroup smartGroup)
        {
            await _smartGroupsRepository.UpdateAsync(smartGroup);
        }

        public async Task DeleteSmartGroupAsync(SmartGroup smartGroup)
        {
            await _smartGroupsRepository.DeleteAsync(smartGroup);
        }

        public bool GroupNameExists(string name, int id = 0)
        {
            return _smartGroupsRepository.Table.Any(sg => sg.Name.Equals(name) && sg.Id != id && !sg.Deleted);
        }

        public virtual async Task<IList<SmartGroupCondition>> GetSmartGroupConditionsBySmartGroupIdAsync(int smartGroupId)
        {
            var query = from u in _smartgroupConditionRepository.Table
                        where u.SmartGroupId == smartGroupId
                        orderby u.Id
                        select u;

            return await query.ToListAsync();
        }

        public virtual async Task InsertSmartGroupConditionAsync(SmartGroupCondition smartGroupCondition)
        {
            await _smartgroupConditionRepository.InsertAsync(smartGroupCondition);
        }

        public virtual async Task<SmartGroupCondition> GetSmartGroupConditionByIdAsync(int id)
        {
            return await _smartgroupConditionRepository.GetByIdAsync(id, cache => default);
        }

        public virtual async Task UpdateSmartGroupConditionAsync(SmartGroupCondition smartGroupCondition)
        {
            await _smartgroupConditionRepository.UpdateAsync(smartGroupCondition);
        }

        public virtual async Task DeleteSmartGroupConditionAsync(SmartGroupCondition smartGroupCondition)
        {
            await _smartgroupConditionRepository.DeleteAsync(smartGroupCondition);
        }

        #endregion
    }
}
