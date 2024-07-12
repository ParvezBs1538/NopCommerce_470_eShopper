using System.Threading.Tasks;
using NopStation.Plugin.Widgets.Helpdesk.Areas.Admin.Models;
using NopStation.Plugin.Widgets.Helpdesk.Domains;

namespace NopStation.Plugin.Widgets.Helpdesk.Areas.Admin.Factories
{
    public interface IStaffModelFactory
    {
        StaffSearchModel PrepareStaffSearchModel(StaffSearchModel searchModel);

        Task<StaffListModel> PrepareStaffListModelAsync(StaffSearchModel searchModel);

        StaffModel PrepareStaffModel(StaffModel model, HelpdeskStaff staff, bool excludeProperties = false);
    }
}
