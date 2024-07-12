using System.Threading.Tasks;
using Nop.Core;
using NopStation.Plugin.Widgets.Helpdesk.Domains;

namespace NopStation.Plugin.Widgets.Helpdesk.Services
{
    public interface IStaffService
    {
        Task DeleteHelpdeskStaffAsync(HelpdeskStaff helpdeskStaff);

        Task InsertHelpdeskStaffAsync(HelpdeskStaff helpdeskStaff);

        Task UpdateHelpdeskStaffAsync(HelpdeskStaff helpdeskStaff);

        Task<HelpdeskStaff> GetHelpdeskStaffByIdAsync(int helpdeskStaffId);

        Task<IPagedList<HelpdeskStaff>> GetAllHelpdeskStaffsAsync(int pageIndex = 0, int pageSize = int.MaxValue);
    }
}