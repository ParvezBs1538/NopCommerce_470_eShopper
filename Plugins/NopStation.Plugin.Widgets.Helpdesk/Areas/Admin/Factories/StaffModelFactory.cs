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
    public class StaffModelFactory : IStaffModelFactory
    {
        #region Fields

        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IStaffService _staffService;

        #endregion

        #region Ctor

        public StaffModelFactory(ILanguageService languageService,
            ILocalizationService localizationService,
            IStaffService staffService)
        {
            _languageService = languageService;
            _localizationService = localizationService;
            _staffService = staffService;
        }

        #endregion

        #region Methods

        public virtual StaffSearchModel PrepareStaffSearchModel(StaffSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.SetGridPageSize();

            return searchModel;
        }

        public virtual async Task<StaffListModel> PrepareStaffListModelAsync(StaffSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var staffs = await _staffService.GetAllHelpdeskStaffsAsync(pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            var model = new StaffListModel().PrepareToGrid(searchModel, staffs, () =>
            {
                return staffs.Select(staff =>
                {
                    var staffModel = PrepareStaffModel(null, staff, true);

                    return staffModel;
                });
            });

            return model;
        }

        public virtual StaffModel PrepareStaffModel(StaffModel model, HelpdeskStaff staff, bool excludeProperties = false)
        {
            if (staff != null)
            {
                if (model == null)
                {
                    model = staff.ToModel<StaffModel>();
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
