using System.Threading.Tasks;
using NopStation.Plugin.Widgets.Helpdesk.Areas.Admin.Models;
using NopStation.Plugin.Widgets.Helpdesk.Domains;

namespace NopStation.Plugin.Widgets.Helpdesk.Areas.Admin.Factories
{
    public interface IDepartmentModelFactory
    {
        DepartmentSearchModel PrepareDepartmentSearchModel(DepartmentSearchModel searchModel);

        Task<DepartmentListModel> PrepareDepartmentListModelAsync(DepartmentSearchModel searchModel);

        DepartmentModel PrepareDepartmentModel(DepartmentModel model, HelpdeskDepartment department, bool excludeProperties = false);
    }
}
