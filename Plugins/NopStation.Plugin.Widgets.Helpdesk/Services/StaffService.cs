using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using NopStation.Plugin.Widgets.Helpdesk.Domains;

namespace NopStation.Plugin.Widgets.Helpdesk.Services
{
    public class StaffService : IStaffService
    {
        #region Fields

        private readonly IRepository<HelpdeskStaff> _helpdeskStaffRepository;

        #endregion

        #region Ctor

        public StaffService(IRepository<HelpdeskStaff> helpdeskStaffRepository)
        {
            _helpdeskStaffRepository = helpdeskStaffRepository;
        }

        #endregion

        #region Methods

        public async Task DeleteHelpdeskStaffAsync(HelpdeskStaff helpdeskStaff)
        {
            await _helpdeskStaffRepository.DeleteAsync(helpdeskStaff);
        }

        public async Task InsertHelpdeskStaffAsync(HelpdeskStaff helpdeskStaff)
        {
            await _helpdeskStaffRepository.InsertAsync(helpdeskStaff);
        }

        public async Task UpdateHelpdeskStaffAsync(HelpdeskStaff helpdeskStaff)
        {
            await _helpdeskStaffRepository.UpdateAsync(helpdeskStaff);
        }

        public async Task<HelpdeskStaff> GetHelpdeskStaffByIdAsync(int helpdeskStaffId)
        {
            if (helpdeskStaffId == 0)
                return null;

            return await _helpdeskStaffRepository.GetByIdAsync(helpdeskStaffId, cache => default);
        }

        public async Task<IPagedList<HelpdeskStaff>> GetAllHelpdeskStaffsAsync(int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _helpdeskStaffRepository.Table;

            query = query.OrderByDescending(e => e.Id);

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        #endregion
    }
}
