using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Services.Localization;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.Widgets.Helpdesk.Areas.Admin.Models;
using NopStation.Plugin.Widgets.Helpdesk.Domains;
using NopStation.Plugin.Widgets.Helpdesk.Services;

namespace NopStation.Plugin.Widgets.Helpdesk.Areas.Admin.Factories
{
    public class DepartmentModelFactory : IDepartmentModelFactory
    {
        #region Fields

        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IDepartmentService _departmentService;

        #endregion

        #region Ctor

        public DepartmentModelFactory(ILanguageService languageService,
            ILocalizationService localizationService,
            IDepartmentService departmentService)
        {
            _languageService = languageService;
            _localizationService = localizationService;
            _departmentService = departmentService;
        }

        #endregion

        #region Methods

        public virtual DepartmentSearchModel PrepareDepartmentSearchModel(DepartmentSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.SetGridPageSize();

            return searchModel;
        }

        public virtual async Task<DepartmentListModel> PrepareDepartmentListModelAsync(DepartmentSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var departments = await _departmentService.GetAllHelpdeskDepartmentsAsync(pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            var model = new DepartmentListModel().PrepareToGrid(searchModel, departments, () =>
            {
                return departments.Select(department =>
                {
                    var departmentModel = PrepareDepartmentModel(null, department, true);

                    return departmentModel;
                });
            });

            return model;
        }

        public virtual DepartmentModel PrepareDepartmentModel(DepartmentModel model, HelpdeskDepartment department, bool excludeProperties = false)
        {
            if (department != null)
            {
                if (model == null)
                {
                    model = department.ToModel<DepartmentModel>();
                }
            }

            if (!excludeProperties)
            {

            }

            return model;
        }

        #endregion
    }
}
