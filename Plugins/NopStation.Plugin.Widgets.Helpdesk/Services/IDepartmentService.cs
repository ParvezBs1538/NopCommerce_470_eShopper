using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Widgets.Helpdesk.Domains;

namespace NopStation.Plugin.Widgets.Helpdesk.Services
{
    public interface IDepartmentService
    {
        Task DeleteHelpdeskDepartmentAsync(HelpdeskDepartment helpdeskDepartment);

        Task InsertHelpdeskDepartmentAsync(HelpdeskDepartment helpdeskDepartment);

        Task UpdateHelpdeskDepartmentAsync(HelpdeskDepartment helpdeskDepartment);

        Task<HelpdeskDepartment> GetHelpdeskDepartmentByIdAsync(int helpdeskDepartmentId);

        Task<IPagedList<HelpdeskDepartment>> GetAllHelpdeskDepartmentsAsync(int pageIndex = 0, int pageSize = int.MaxValue);
    }
}