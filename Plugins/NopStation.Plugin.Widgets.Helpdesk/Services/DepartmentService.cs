using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using NopStation.Plugin.Widgets.Helpdesk.Domains;

namespace NopStation.Plugin.Widgets.Helpdesk.Services
{
    public class DepartmentService : IDepartmentService
    {
        #region Fields

        private readonly IRepository<HelpdeskDepartment> _helpdeskDepartmentRepository;

        #endregion

        #region Ctor

        public DepartmentService(IRepository<HelpdeskDepartment> helpdeskDepartmentRepository)
        {
            _helpdeskDepartmentRepository = helpdeskDepartmentRepository;
        }

        #endregion

        #region Methods

        public async Task DeleteHelpdeskDepartmentAsync(HelpdeskDepartment helpdeskDepartment)
        {
            await _helpdeskDepartmentRepository.DeleteAsync(helpdeskDepartment);
        }

        public async Task InsertHelpdeskDepartmentAsync(HelpdeskDepartment helpdeskDepartment)
        {
            await _helpdeskDepartmentRepository.InsertAsync(helpdeskDepartment);
        }

        public async Task UpdateHelpdeskDepartmentAsync(HelpdeskDepartment helpdeskDepartment)
        {
            await _helpdeskDepartmentRepository.UpdateAsync(helpdeskDepartment);
        }

        public async Task<HelpdeskDepartment> GetHelpdeskDepartmentByIdAsync(int helpdeskDepartmentId)
        {
            if (helpdeskDepartmentId == 0)
                return null;

            return await _helpdeskDepartmentRepository.GetByIdAsync(helpdeskDepartmentId, cache => default);
        }

        public async Task<IPagedList<HelpdeskDepartment>> GetAllHelpdeskDepartmentsAsync(int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _helpdeskDepartmentRepository.Table;

            query = query.OrderByDescending(e => e.Id);

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        #endregion
    }
}
